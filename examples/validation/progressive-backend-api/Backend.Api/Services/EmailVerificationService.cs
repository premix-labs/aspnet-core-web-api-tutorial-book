using System.Security.Cryptography;
using System.Text;
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

public class EmailVerificationService(
    AppDbContext db,
    IEmailSender emailSender,
    AuditLogService auditLogService,
    IOptions<EmailVerificationOptions> options)
{
    private readonly EmailVerificationOptions optionsValue = options.Value;

    public async Task SendVerificationAsync(
        User user,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (user.IsEmailVerified)
        {
            return;
        }

        var utcNow = DateTimeOffset.UtcNow;
        var activeTokens = await db.EmailVerificationTokens
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

        db.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            CreatedByIp = ipAddress,
            CreatedAt = utcNow,
            ExpiresAt = utcNow.AddHours(optionsValue.ExpirationHours)
        });

        await db.SaveChangesAsync(cancellationToken);
        await auditLogService.LogAsync(
            user.Id,
            AuditActions.EmailVerificationRequested,
            nameof(User),
            user.Id.ToString(),
            ipAddress,
            "Email verification token issued.");

        await emailSender.SendAsync(
            new EmailMessage(
                user.Email,
                "Verify your email",
                BuildEmailBody(rawToken)),
            cancellationToken);
    }

    public async Task<CurrentUserResponse> VerifyAsync(
        VerifyEmailRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.Token);
        var token = await db.EmailVerificationTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        var utcNow = DateTimeOffset.UtcNow;

        if (token is null || !token.IsActive(utcNow))
        {
            throw new UnauthorizedException(
                "Email verification token is invalid or expired",
                "EMAIL_VERIFICATION_TOKEN_INVALID");
        }

        if (!token.User.IsActive)
        {
            throw new ForbiddenException(
                "User account is inactive",
                "USER_INACTIVE");
        }

        token.UsedAt = utcNow;
        token.User.IsEmailVerified = true;
        token.User.EmailVerifiedAt = utcNow;
        token.User.UpdatedAt = utcNow;

        var otherTokens = await db.EmailVerificationTokens
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

        await db.SaveChangesAsync(cancellationToken);
        await auditLogService.LogAsync(
            token.User.Id,
            AuditActions.EmailVerified,
            nameof(User),
            token.User.Id.ToString(),
            ipAddress,
            "User email verified.");

        return new CurrentUserResponse(
            token.User.Id,
            token.User.Email,
            token.User.Role,
            token.User.IsActive,
            token.User.IsEmailVerified);
    }

    public async Task ResendAsync(
        ResendEmailVerificationRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await db.Users
            .FirstOrDefaultAsync(
                user => user.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (user is null || user.IsEmailVerified)
        {
            return;
        }

        await SendVerificationAsync(user, ipAddress, cancellationToken);
    }

    private string BuildEmailBody(string rawToken)
    {
        var url = optionsValue.VerificationUrlTemplate.Replace(
            "{token}",
            Uri.EscapeDataString(rawToken),
            StringComparison.Ordinal);

        return $"Open this link to verify your email: {url}";
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
