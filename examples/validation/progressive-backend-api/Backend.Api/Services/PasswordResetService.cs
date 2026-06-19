using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Backend.Api.Constants;
using Backend.Api.Data;
using Backend.Api.Dtos.Auth;
using Backend.Api.Exceptions;
using Backend.Api.Models;
using Backend.Api.Options;

namespace Backend.Api.Services;

public class PasswordResetService(
    AppDbContext db,
    IPasswordHasher<User> passwordHasher,
    IEmailSender emailSender,
    RefreshTokenService refreshTokenService,
    AuditLogService auditLogService,
    IOptions<PasswordResetOptions> options)
{
    private readonly PasswordResetOptions optionsValue = options.Value;

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, string? ipAddress)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await db.Users
            .FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail);

        if (user is null || !user.IsActive)
        {
            return;
        }

        var utcNow = DateTimeOffset.UtcNow;
        var activeTokens = await db.PasswordResetTokens
            .Where(token =>
                token.UserId == user.Id &&
                token.UsedAt == null &&
                token.RevokedAt == null)
            .ToListAsync();

        foreach (var activeToken in activeTokens)
        {
            activeToken.RevokedAt = utcNow;
        }

        var rawToken = GenerateToken();
        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            CreatedByIp = ipAddress,
            CreatedAt = utcNow,
            ExpiresAt = utcNow.AddHours(optionsValue.ExpirationHours)
        });

        await db.SaveChangesAsync();
        await auditLogService.LogAsync(
            user.Id,
            AuditActions.PasswordResetRequested,
            nameof(User),
            user.Id.ToString(),
            ipAddress,
            "Password reset token issued.");

        await emailSender.SendAsync(
            new EmailMessage(
                user.Email,
                "Reset your password",
                BuildEmailBody(rawToken)),
            CancellationToken.None);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, string? ipAddress)
    {
        var tokenHash = HashToken(request.Token);
        var token = await db.PasswordResetTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash);

        var utcNow = DateTimeOffset.UtcNow;

        if (token is null || !token.IsActive(utcNow))
        {
            throw new UnauthorizedException(
                "Password reset token is invalid or expired",
                "PASSWORD_RESET_TOKEN_INVALID");
        }

        if (!token.User.IsActive)
        {
            throw new ForbiddenException(
                "User account is inactive",
                "USER_INACTIVE");
        }

        token.UsedAt = utcNow;
        token.User.PasswordHash = passwordHasher.HashPassword(token.User, request.NewPassword);
        token.User.PasswordChangedAt = utcNow;
        token.User.AccessFailedCount = 0;
        token.User.LockoutEnd = null;
        token.User.UpdatedAt = utcNow;

        var otherTokens = await db.PasswordResetTokens
            .Where(otherToken =>
                otherToken.UserId == token.UserId &&
                otherToken.Id != token.Id &&
                otherToken.UsedAt == null &&
                otherToken.RevokedAt == null)
            .ToListAsync();

        foreach (var otherToken in otherTokens)
        {
            otherToken.RevokedAt = utcNow;
        }

        await refreshTokenService.RevokeAllForUserAsync(
            token.UserId,
            ipAddress,
            "PasswordReset");

        await db.SaveChangesAsync();
        await auditLogService.LogAsync(
            token.UserId,
            AuditActions.PasswordResetCompleted,
            nameof(User),
            token.UserId.ToString(),
            ipAddress,
            "Password reset completed and refresh tokens revoked.");
    }

    private string BuildEmailBody(string rawToken)
    {
        var url = optionsValue.ResetUrlTemplate.Replace(
            "{token}",
            Uri.EscapeDataString(rawToken),
            StringComparison.Ordinal);

        return $"Open this link to reset your password: {url}";
    }

    private static string GenerateToken()
    {
        return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
}
