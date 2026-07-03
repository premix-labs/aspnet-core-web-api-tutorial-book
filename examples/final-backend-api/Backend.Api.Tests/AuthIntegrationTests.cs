using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Backend.Api.Constants;
using Backend.Api.Data;
using Backend.Api.Dtos.Auth;

namespace Backend.Api.Tests;

public class AuthIntegrationTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Me_WhenNoToken_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WhenEmailInvalid_ReturnsBadRequest()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "not-an-email",
            password = "Passw0rd!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await ReadJsonAsync(response);
        Assert.Equal("VALIDATION_FAILED", problemDetails.GetProperty("code").GetString());
        Assert.True(problemDetails.TryGetProperty("traceId", out _));
        Assert.False(problemDetails.TryGetProperty("stackTrace", out _));
    }

    [Fact]
    public async Task RegisterLoginAndMe_WhenDataIsValid_ReturnsCurrentUser()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "reader@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);
        Assert.False(registerBody.User.IsEmailVerified);

        var verificationToken = GetVerificationToken("reader@example.com");
        var verifyResponse = await client.PostAsJsonAsync("/api/v1/auth/verify-email", new
        {
            token = verificationToken
        });

        verifyResponse.EnsureSuccessStatusCode();

        var verifiedUser = await verifyResponse.Content.ReadFromJsonAsync<CurrentUserResponse>();
        Assert.NotNull(verifiedUser);
        Assert.True(verifiedUser.IsEmailVerified);

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "reader@example.com",
            password = "Passw0rd!"
        });

        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);
        Assert.False(string.IsNullOrWhiteSpace(loginBody.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(loginBody.RefreshToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginBody.AccessToken);

        var currentUser = await client.GetFromJsonAsync<CurrentUserResponse>("/api/v1/auth/me");

        Assert.NotNull(currentUser);
        Assert.Equal("reader@example.com", currentUser.Email);
        Assert.Equal("User", currentUser.Role);
        Assert.True(currentUser.IsActive);
        Assert.True(currentUser.IsEmailVerified);
    }

    [Fact]
    public async Task Register_WhenDataIsValid_SendsVerificationEmail()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "verify-email@example.com",
            password = "Passw0rd!"
        });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(body.User.IsEmailVerified);
        Assert.Contains(
            factory.EmailSender.Messages,
            message =>
                message.To == "verify-email@example.com" &&
                message.Body.Contains("token=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task VerifyEmail_WhenTokenIsInvalid_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/verify-email", new
        {
            token = "invalid-token"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ResendEmailVerification_WhenUserIsNotVerified_SendsNewEmail()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "resend@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var beforeCount = factory.EmailSender.Messages.Count(message => message.To == "resend@example.com");

        var resendResponse = await client.PostAsJsonAsync("/api/v1/auth/resend-email-verification", new
        {
            email = "resend@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, resendResponse.StatusCode);

        var afterCount = factory.EmailSender.Messages.Count(message => message.To == "resend@example.com");
        Assert.True(afterCount > beforeCount);
    }

    [Fact]
    public async Task Register_WhenVerificationEmailIsSent_StoresOnlyTokenHash()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "verification-hash@example.com",
            password = "Passw0rd!"
        });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);

        var rawToken = GetVerificationToken("verification-hash@example.com");

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storedToken = await dbContext.EmailVerificationTokens
            .AsNoTracking()
            .SingleAsync(token => token.UserId == body.User.Id);

        Assert.NotEqual(rawToken, storedToken.TokenHash);
        Assert.Equal(44, storedToken.TokenHash.Length);
        Assert.Null(storedToken.UsedAt);
        Assert.Null(storedToken.RevokedAt);
        Assert.True(storedToken.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task VerifyEmail_WhenTokenIsReused_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "verify-reuse@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var verificationToken = GetVerificationToken("verify-reuse@example.com");
        var verifyResponse = await client.PostAsJsonAsync("/api/v1/auth/verify-email", new
        {
            token = verificationToken
        });

        verifyResponse.EnsureSuccessStatusCode();

        var reusedResponse = await client.PostAsJsonAsync("/api/v1/auth/verify-email", new
        {
            token = verificationToken
        });

        Assert.Equal(HttpStatusCode.Unauthorized, reusedResponse.StatusCode);
    }

    [Fact]
    public async Task VerifyEmail_WhenTokenIsExpired_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "verify-expired@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var verificationToken = GetVerificationToken("verify-expired@example.com");

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storedToken = await dbContext.EmailVerificationTokens
            .SingleAsync(token => token.User.Email == "verify-expired@example.com");
        storedToken.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        await dbContext.SaveChangesAsync();

        var verifyResponse = await client.PostAsJsonAsync("/api/v1/auth/verify-email", new
        {
            token = verificationToken
        });

        Assert.Equal(HttpStatusCode.Unauthorized, verifyResponse.StatusCode);
    }

    [Fact]
    public async Task ResendEmailVerification_WhenCalled_RevokesPreviousToken()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "resend-revokes@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var firstToken = GetVerificationToken("resend-revokes@example.com");

        var resendResponse = await client.PostAsJsonAsync("/api/v1/auth/resend-email-verification", new
        {
            email = "resend-revokes@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, resendResponse.StatusCode);

        var firstVerifyResponse = await client.PostAsJsonAsync("/api/v1/auth/verify-email", new
        {
            token = firstToken
        });

        Assert.Equal(HttpStatusCode.Unauthorized, firstVerifyResponse.StatusCode);

        var secondToken = GetVerificationToken("resend-revokes@example.com");
        var secondVerifyResponse = await client.PostAsJsonAsync("/api/v1/auth/verify-email", new
        {
            token = secondToken
        });

        secondVerifyResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ResendEmailVerification_WhenEmailDoesNotExistOrIsVerified_DoesNotSendEmail()
    {
        var client = factory.CreateClient();

        var missingBeforeCount = factory.EmailSender.Messages.Count;
        var missingResponse = await client.PostAsJsonAsync("/api/v1/auth/resend-email-verification", new
        {
            email = "missing-verification@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, missingResponse.StatusCode);
        Assert.Equal(missingBeforeCount, factory.EmailSender.Messages.Count);

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "already-verified-resend@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var verificationToken = GetVerificationToken("already-verified-resend@example.com");
        var verifyResponse = await client.PostAsJsonAsync("/api/v1/auth/verify-email", new
        {
            token = verificationToken
        });

        verifyResponse.EnsureSuccessStatusCode();

        var verifiedBeforeCount = factory.EmailSender.Messages.Count;
        var resendVerifiedResponse = await client.PostAsJsonAsync("/api/v1/auth/resend-email-verification", new
        {
            email = "already-verified-resend@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, resendVerifiedResponse.StatusCode);
        Assert.Equal(verifiedBeforeCount, factory.EmailSender.Messages.Count);
    }

    [Fact]
    public async Task VerifyEmail_WhenSuccessful_WritesAuditLogAndConsumesToken()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "verify-audit@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        var verificationToken = GetVerificationToken("verify-audit@example.com");
        var verifyResponse = await client.PostAsJsonAsync("/api/v1/auth/verify-email", new
        {
            token = verificationToken
        });

        verifyResponse.EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storedToken = await dbContext.EmailVerificationTokens
            .AsNoTracking()
            .SingleAsync(token => token.UserId == registerBody.User.Id);
        var auditLogs = await dbContext.AuditLogs
            .AsNoTracking()
            .Where(log => log.ActorUserId == registerBody.User.Id)
            .ToListAsync();

        Assert.NotNull(storedToken.UsedAt);
        Assert.Contains(auditLogs, log => log.Action == AuditActions.EmailVerificationRequested);
        Assert.Contains(auditLogs, log => log.Action == AuditActions.EmailVerified);
        Assert.DoesNotContain(auditLogs, log => log.Detail?.Contains(verificationToken, StringComparison.Ordinal) == true);
    }

    [Fact]
    public async Task ForgotPassword_WhenUserExists_SendsResetEmail()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "forgot@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = "forgot@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Contains(
            factory.EmailSender.Messages,
            message =>
                message.To == "forgot@example.com" &&
                message.Subject == "Reset your password" &&
                message.Body.Contains("token=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailDoesNotExistOrUserIsInactive_DoesNotSendEmail()
    {
        var client = factory.CreateClient();

        var missingBeforeCount = factory.EmailSender.Messages.Count;
        var missingResponse = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = "missing-reset@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, missingResponse.StatusCode);
        Assert.Equal(missingBeforeCount, factory.EmailSender.Messages.Count);

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "inactive-reset@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.SingleAsync(user => user.Id == registerBody.User.Id);
        user.IsActive = false;
        await dbContext.SaveChangesAsync();

        var inactiveBeforeCount = factory.EmailSender.Messages.Count;
        var inactiveResponse = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = "inactive-reset@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, inactiveResponse.StatusCode);
        Assert.Equal(inactiveBeforeCount, factory.EmailSender.Messages.Count);
    }

    [Fact]
    public async Task ForgotPassword_WhenResetEmailIsSent_StoresOnlyTokenHash()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "reset-hash@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        var forgotResponse = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = "reset-hash@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, forgotResponse.StatusCode);

        var rawToken = GetEmailToken("reset-hash@example.com", "Reset your password");

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storedToken = await dbContext.PasswordResetTokens
            .AsNoTracking()
            .SingleAsync(token => token.UserId == registerBody.User.Id);

        Assert.NotEqual(rawToken, storedToken.TokenHash);
        Assert.Equal(44, storedToken.TokenHash.Length);
        Assert.Null(storedToken.UsedAt);
        Assert.Null(storedToken.RevokedAt);
        Assert.True(storedToken.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task ForgotPassword_WhenCalledAgain_RevokesPreviousResetToken()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "reset-revokes@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var firstForgotResponse = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = "reset-revokes@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, firstForgotResponse.StatusCode);

        var firstToken = GetEmailToken("reset-revokes@example.com", "Reset your password");

        var secondForgotResponse = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = "reset-revokes@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, secondForgotResponse.StatusCode);

        var firstResetResponse = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = firstToken,
            newPassword = "NewPassw0rd!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, firstResetResponse.StatusCode);

        var secondToken = GetEmailToken("reset-revokes@example.com", "Reset your password");
        var secondResetResponse = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = secondToken,
            newPassword = "NewPassw0rd!"
        });

        Assert.Equal(HttpStatusCode.NoContent, secondResetResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenTokenIsInvalid_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = "invalid-token",
            newPassword = "NewPassw0rd!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenTokenIsExpired_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "reset-expired@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        var forgotResponse = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = "reset-expired@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, forgotResponse.StatusCode);

        var resetToken = GetEmailToken("reset-expired@example.com", "Reset your password");

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storedToken = await dbContext.PasswordResetTokens
            .SingleAsync(token => token.UserId == registerBody.User.Id);
        storedToken.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        await dbContext.SaveChangesAsync();

        var resetResponse = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = resetToken,
            newPassword = "NewPassw0rd!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, resetResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenTokenIsValid_ChangesPasswordAndRevokesRefreshTokens()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "reset-success@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        var forgotResponse = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = "reset-success@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, forgotResponse.StatusCode);

        var resetToken = GetEmailToken("reset-success@example.com", "Reset your password");
        var resetResponse = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = resetToken,
            newPassword = "NewPassw0rd!"
        });

        Assert.Equal(HttpStatusCode.NoContent, resetResponse.StatusCode);

        var oldPasswordResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "reset-success@example.com",
            password = "Passw0rd!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, oldPasswordResponse.StatusCode);

        var newPasswordResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "reset-success@example.com",
            password = "NewPassw0rd!"
        });

        newPasswordResponse.EnsureSuccessStatusCode();

        var oldRefreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = registerBody.RefreshToken
        });

        Assert.Equal(HttpStatusCode.Unauthorized, oldRefreshResponse.StatusCode);

        var reusedTokenResponse = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = resetToken,
            newPassword = "AnotherPassw0rd!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, reusedTokenResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WhenSuccessful_ConsumesTokenResetsLockoutAndWritesAuditLog()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "reset-audit@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var failedResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
            {
                email = "reset-audit@example.com",
                password = "wrong-password"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, failedResponse.StatusCode);
        }

        var forgotResponse = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = "reset-audit@example.com"
        });

        Assert.Equal(HttpStatusCode.NoContent, forgotResponse.StatusCode);

        var resetToken = GetEmailToken("reset-audit@example.com", "Reset your password");
        var resetResponse = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = resetToken,
            newPassword = "NewPassw0rd!"
        });

        Assert.Equal(HttpStatusCode.NoContent, resetResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(user => user.Id == registerBody.User.Id);
        var storedToken = await dbContext.PasswordResetTokens
            .AsNoTracking()
            .SingleAsync(token => token.UserId == registerBody.User.Id);
        var auditLogs = await dbContext.AuditLogs
            .AsNoTracking()
            .Where(log => log.ActorUserId == registerBody.User.Id)
            .ToListAsync();

        Assert.Equal(0, user.AccessFailedCount);
        Assert.Null(user.LockoutEnd);
        Assert.NotNull(user.PasswordChangedAt);
        Assert.NotNull(storedToken.UsedAt);
        Assert.Contains(auditLogs, log => log.Action == AuditActions.PasswordResetRequested);
        Assert.Contains(auditLogs, log => log.Action == AuditActions.PasswordResetCompleted);
        Assert.DoesNotContain(auditLogs, log => log.Detail?.Contains(resetToken, StringComparison.Ordinal) == true);
    }

    [Fact]
    public async Task Register_WhenEmailOnlyDiffersByCase_ReturnsConflict()
    {
        var client = factory.CreateClient();

        var firstResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "CaseSensitive@example.com",
            password = "Passw0rd!"
        });

        firstResponse.EnsureSuccessStatusCode();

        var duplicateResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "casesensitive@example.com",
            password = "Passw0rd!"
        });

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var problemDetails = await ReadJsonAsync(duplicateResponse);
        Assert.Equal("EMAIL_ALREADY_EXISTS", problemDetails.GetProperty("code").GetString());
        Assert.True(problemDetails.TryGetProperty("traceId", out _));
        Assert.False(problemDetails.TryGetProperty("stackTrace", out _));
    }

    [Fact]
    public async Task Refresh_WhenTokenIsUsed_RotatesRefreshToken()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "refresh@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        var refreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = registerBody.RefreshToken
        });

        refreshResponse.EnsureSuccessStatusCode();

        var refreshBody = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(refreshBody);
        Assert.False(string.IsNullOrWhiteSpace(refreshBody.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshBody.RefreshToken));
        Assert.NotEqual(registerBody.RefreshToken, refreshBody.RefreshToken);

        var oldTokenResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = registerBody.RefreshToken
        });

        Assert.Equal(HttpStatusCode.Unauthorized, oldTokenResponse.StatusCode);

        var revokedFamilyTokenResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = refreshBody.RefreshToken
        });

        Assert.Equal(HttpStatusCode.Unauthorized, revokedFamilyTokenResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storedTokens = await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(token => token.UserId == registerBody.User.Id)
            .ToListAsync();

        var rotatedToken = Assert.Single(storedTokens, token => token.RevocationReason == "Rotated");
        var tokenFamily = storedTokens
            .Where(token => token.FamilyId == rotatedToken.FamilyId)
            .ToList();

        Assert.Contains(tokenFamily, token => token.RevocationReason == "ReuseDetected");
        Assert.All(tokenFamily, token => Assert.NotNull(token.RevokedAt));

        Assert.Contains(
            await dbContext.AuditLogs.AsNoTracking().ToListAsync(),
            log =>
                log.ActorUserId == registerBody.User.Id &&
                log.Action == AuditActions.RefreshTokenReuseDetected);
    }

    [Fact]
    public async Task Login_WhenUserAgentIsProvided_StoresRefreshTokenUserAgent()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("BackendApiBookTests/1.0");

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "session-user-agent@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storedToken = await dbContext.RefreshTokens
            .AsNoTracking()
            .SingleAsync(token => token.UserId == registerBody.User.Id);

        Assert.Equal("BackendApiBookTests/1.0", storedToken.UserAgent);
    }

    [Fact]
    public async Task Sessions_WhenAuthenticated_ReturnsActiveSessions()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("SessionListTests/1.0");

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "sessions-list@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var loginBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginBody.AccessToken);

        var sessions = await client.GetFromJsonAsync<IReadOnlyList<AuthSessionResponse>>(
            "/api/v1/auth/sessions");

        Assert.NotNull(sessions);
        Assert.NotEmpty(sessions);
        Assert.Contains(sessions, session => session.UserAgent == "SessionListTests/1.0");
    }

    [Fact]
    public async Task RevokeSession_WhenFamilyBelongsToCurrentUser_RevokesOnlyThatSession()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "revoke-one@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "revoke-one@example.com",
            password = "Passw0rd!"
        });

        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);

        var otherRegisterResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "revoke-other@example.com",
            password = "Passw0rd!"
        });

        otherRegisterResponse.EnsureSuccessStatusCode();

        var otherBody = await otherRegisterResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(otherBody);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginBody.AccessToken);

        var ownSessions = await client.GetFromJsonAsync<IReadOnlyList<AuthSessionResponse>>(
            "/api/v1/auth/sessions");
        Assert.NotNull(ownSessions);
        var ownFamilyId = ownSessions.First().FamilyId;

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var otherFamilyId = await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(token => token.UserId == otherBody.User.Id)
            .Select(token => token.FamilyId)
            .SingleAsync();

        var otherRevokeResponse = await client.DeleteAsync($"/api/v1/auth/sessions/{otherFamilyId}");
        Assert.Equal(HttpStatusCode.NoContent, otherRevokeResponse.StatusCode);

        var otherRefreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = otherBody.RefreshToken
        });
        otherRefreshResponse.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginBody.AccessToken);

        var revokeResponse = await client.DeleteAsync($"/api/v1/auth/sessions/{ownFamilyId}");
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        var revokedRefreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = loginBody.RefreshToken
        });

        Assert.Equal(HttpStatusCode.Unauthorized, revokedRefreshResponse.StatusCode);
        Assert.Contains(
            await dbContext.AuditLogs.AsNoTracking().ToListAsync(),
            log =>
                log.ActorUserId == loginBody.User.Id &&
                log.Action == AuditActions.RefreshTokenSessionRevoked &&
                log.EntityId == ownFamilyId.ToString());
    }

    [Fact]
    public async Task RevokeAllSessions_WhenCalled_RevokesAllCurrentUserSessions()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "revoke-all@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "revoke-all@example.com",
            password = "Passw0rd!"
        });

        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginBody.AccessToken);

        var revokeResponse = await client.DeleteAsync("/api/v1/auth/sessions");
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        foreach (var refreshToken in new[] { registerBody.RefreshToken, loginBody.RefreshToken })
        {
            var refreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
            {
                refreshToken
            });

            Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
        }

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Contains(
            await dbContext.AuditLogs.AsNoTracking().ToListAsync(),
            log =>
                log.ActorUserId == loginBody.User.Id &&
                log.Action == AuditActions.AllRefreshTokenSessionsRevoked);
    }

    [Fact]
    public async Task Login_WhenPasswordFailsRepeatedly_LocksAccount()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "locked@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var failedResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
            {
                email = "locked@example.com",
                password = "wrong-password"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, failedResponse.StatusCode);
        }

        var lockedResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "locked@example.com",
            password = "Passw0rd!"
        });

        Assert.Equal(HttpStatusCode.Forbidden, lockedResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_UsesGenericError()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "generic-login-error@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var wrongPasswordResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "generic-login-error@example.com",
            password = "wrong-password"
        });

        var missingEmailResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "missing-generic-login-error@example.com",
            password = "wrong-password"
        });

        var wrongPasswordBody = await wrongPasswordResponse.Content.ReadAsStringAsync();
        var missingEmailBody = await missingEmailResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPasswordResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, missingEmailResponse.StatusCode);
        Assert.Contains("Invalid email or password", wrongPasswordBody, StringComparison.Ordinal);
        Assert.Contains("Invalid email or password", missingEmailBody, StringComparison.Ordinal);
        Assert.DoesNotContain("generic-login-error@example.com", wrongPasswordBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("missing-generic-login-error@example.com", missingEmailBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WhenPasswordFails_WritesAuditLogAndSuccessfulLoginResetsFailedCount()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "failed-login-audit@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        for (var attempt = 1; attempt <= 2; attempt++)
        {
            var failedResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
            {
                email = "failed-login-audit@example.com",
                password = "wrong-password"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, failedResponse.StatusCode);
        }

        var successResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "failed-login-audit@example.com",
            password = "Passw0rd!"
        });

        successResponse.EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(user => user.Id == registerBody.User.Id);
        var auditLogs = await dbContext.AuditLogs
            .AsNoTracking()
            .Where(log => log.ActorUserId == registerBody.User.Id)
            .ToListAsync();

        Assert.Equal(0, user.AccessFailedCount);
        Assert.Null(user.LockoutEnd);
        Assert.NotNull(user.LastLoginAt);
        Assert.True(auditLogs.Count(log => log.Action == AuditActions.LoginFailed) >= 2);
        Assert.Contains(auditLogs, log => log.Action == AuditActions.LoginSucceeded);
    }

    [Fact]
    public async Task Login_WhenAccountIsLocked_WritesAccountLockedAuditLog()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "locked-audit@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var failedResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
            {
                email = "locked-audit@example.com",
                password = "wrong-password"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, failedResponse.StatusCode);
        }

        var lockedResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "locked-audit@example.com",
            password = "Passw0rd!"
        });

        Assert.Equal(HttpStatusCode.Forbidden, lockedResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(user => user.Id == registerBody.User.Id);
        var auditLogs = await dbContext.AuditLogs
            .AsNoTracking()
            .Where(log => log.ActorUserId == registerBody.User.Id)
            .ToListAsync();

        Assert.NotNull(user.LockoutEnd);
        Assert.Contains(auditLogs, log => log.Action == AuditActions.AccountLocked);
        Assert.Contains(auditLogs, log => log.Action == AuditActions.LoginFailed);
    }

    [Fact]
    public async Task AdminUsers_WhenNoToken_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/admin/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminUsers_WhenSignedInAsAdmin_ReturnsOk()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "admin-authz@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await dbContext.Users.SingleAsync(user => user.Id == registerBody.User.Id);
            user.Role = Roles.Admin;
            await dbContext.SaveChangesAsync();
        }

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin-authz@example.com",
            password = "Passw0rd!"
        });

        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginBody.AccessToken);

        var response = await client.GetAsync("/api/v1/admin/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminUsers_WhenSignedInAsRegularUser_ReturnsForbidden()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "regular@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var loginBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            loginBody.AccessToken);

        var response = await client.GetAsync("/api/v1/admin/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return document.RootElement.Clone();
    }

    private string GetVerificationToken(string email)
    {
        return GetEmailToken(email, "Verify your email");
    }

    private string GetEmailToken(string email, string subject)
    {
        var message = factory.EmailSender.Messages
            .Last(message => message.To == email && message.Subject == subject);

        var match = Regex.Match(message.Body, @"token=([^\s]+)");
        Assert.True(match.Success, "Email should contain token query string.");

        return Uri.UnescapeDataString(match.Groups[1].Value);
    }
}
