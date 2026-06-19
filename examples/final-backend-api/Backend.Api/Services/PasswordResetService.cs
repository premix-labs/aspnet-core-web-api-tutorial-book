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
    AppDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IEmailSender emailSender,
    IRefreshTokenService refreshTokenService,
    IAuditLogService auditLogService,
    IOptions<PasswordResetOptions> options) : IPasswordResetService
{
    private readonly PasswordResetOptions _options = options.Value;

    public async Task ForgotPasswordAsync(
        ForgotPasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await dbContext.Users
            .FirstOrDefaultAsync(
                user => user.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (user is null || !user.IsActive)
        {
            return;
        }

        var utcNow = DateTimeOffset.UtcNow;
        var activeTokens = await dbContext.PasswordResetTokens
            .Where(token =>
                token.UserId == user.Id &&
                token.UsedAt == null &&
                token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var activeToken in activeTokens)
        {
            activeToken.RevokedAt = utcNow;
        }

        var rawToken = GenerateToken();
        dbContext.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            CreatedByIp = ipAddress,
            CreatedAt = utcNow,
            ExpiresAt = utcNow.AddHours(_options.ExpirationHours)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogService.WriteAsync(
            user.Id,
            AuditActions.PasswordResetRequested,
            nameof(User),
            user.Id.ToString(),
            ipAddress,
            "Password reset token issued.",
            cancellationToken);

        await emailSender.SendAsync(
            new EmailMessage(
                user.Email,
                "Reset your password",
                BuildEmailBody(rawToken)),
            cancellationToken);
    }

    public async Task ResetPasswordAsync(
        ResetPasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.Token);
        var token = await dbContext.PasswordResetTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        var utcNow = DateTimeOffset.UtcNow;

        if (token is null || !token.IsActive(utcNow))
        {
            throw new UnauthorizedException(
                "Password reset token is invalid or expired.",
                "PASSWORD_RESET_TOKEN_INVALID");
        }

        if (!token.User.IsActive)
        {
            throw new ForbiddenException("This account is inactive.", "USER_INACTIVE");
        }

        token.UsedAt = utcNow;
        token.User.PasswordHash = passwordHasher.HashPassword(token.User, request.NewPassword);
        token.User.PasswordChangedAt = utcNow;
        token.User.AccessFailedCount = 0;
        token.User.LockoutEnd = null;
        token.User.UpdatedAt = utcNow;

        var otherTokens = await dbContext.PasswordResetTokens
            .Where(otherToken =>
                otherToken.UserId == token.UserId &&
                otherToken.Id != token.Id &&
                otherToken.UsedAt == null &&
                otherToken.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var otherToken in otherTokens)
        {
            otherToken.RevokedAt = utcNow;
        }

        await refreshTokenService.RevokeAllForUserAsync(
            token.UserId,
            ipAddress,
            "PasswordReset",
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogService.WriteAsync(
            token.UserId,
            AuditActions.PasswordResetCompleted,
            nameof(User),
            token.UserId.ToString(),
            ipAddress,
            "Password reset completed and refresh tokens revoked.",
            cancellationToken);
    }

    private string BuildEmailBody(string rawToken)
    {
        var url = _options.ResetUrlTemplate.Replace(
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
