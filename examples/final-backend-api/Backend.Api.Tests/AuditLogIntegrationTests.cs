using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Backend.Api.Constants;
using Backend.Api.Data;
using Backend.Api.Dtos.Auth;

namespace Backend.Api.Tests;

public class AuditLogIntegrationTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task AuthFlow_WhenSecurityEventsHappen_WritesAuditLogs()
    {
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "audit@example.com",
            password = "Passw0rd!"
        });

        registerResponse.EnsureSuccessStatusCode();

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(registerBody);

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "audit@example.com",
            password = "Passw0rd!"
        });

        loginResponse.EnsureSuccessStatusCode();

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody);

        var refreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = loginBody.RefreshToken
        });

        refreshResponse.EnsureSuccessStatusCode();

        var forgotPasswordResponse = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = "audit@example.com"
        });

        forgotPasswordResponse.EnsureSuccessStatusCode();

        var resetToken = GetEmailToken("audit@example.com", "Reset your password");
        var resetPasswordResponse = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = resetToken,
            newPassword = "NewPassw0rd!"
        });

        resetPasswordResponse.EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var auditLogs = await dbContext.AuditLogs
            .AsNoTracking()
            .Where(log => log.EntityId == registerBody.User.Id.ToString())
            .Select(log => new { log.Action, log.ActorUserId, log.IpAddress, log.Detail })
            .ToListAsync();

        Assert.Contains(auditLogs, log => log.Action == AuditActions.UserRegistered);
        Assert.Contains(auditLogs, log => log.Action == AuditActions.EmailVerificationRequested);
        Assert.Contains(auditLogs, log => log.Action == AuditActions.LoginSucceeded);
        Assert.Contains(auditLogs, log => log.Action == AuditActions.RefreshTokenRotated);
        Assert.Contains(auditLogs, log => log.Action == AuditActions.PasswordResetRequested);
        Assert.Contains(auditLogs, log => log.Action == AuditActions.PasswordResetCompleted);
        Assert.All(auditLogs, log => Assert.Equal(registerBody.User.Id, log.ActorUserId));
        Assert.All(auditLogs, log => Assert.False(string.IsNullOrWhiteSpace(log.IpAddress)));
        Assert.DoesNotContain(auditLogs, log => log.Detail?.Contains(registerBody.RefreshToken, StringComparison.Ordinal) == true);
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
