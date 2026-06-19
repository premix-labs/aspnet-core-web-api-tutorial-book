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
    AppDbContext dbContext,
    IEmailSender emailSender,
    IAuditLogService auditLogService,
    IOptions<EmailVerificationOptions> options) : IEmailVerificationService
{
    private readonly EmailVerificationOptions _options = options.Value;

    public async Task SendVerificationAsync(
        User user,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        if (user.IsEmailVerified)
        {
            return;
        }

        var utcNow = DateTimeOffset.UtcNow;
        var activeTokens = await dbContext.EmailVerificationTokens
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
        var token = new EmailVerificationToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            CreatedByIp = ipAddress,
            CreatedAt = utcNow,
            ExpiresAt = utcNow.AddHours(_options.ExpirationHours)
        };

        dbContext.EmailVerificationTokens.Add(token);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogService.WriteAsync(
            user.Id,
            AuditActions.EmailVerificationRequested,
            nameof(User),
            user.Id.ToString(),
            ipAddress,
            "Email verification token issued.",
            cancellationToken);

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
        CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.Token);
        var token = await dbContext.EmailVerificationTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        var utcNow = DateTimeOffset.UtcNow;

        if (token is null || !token.IsActive(utcNow))
        {
            throw new UnauthorizedException(
                "Email verification token is invalid or expired.",
                "EMAIL_VERIFICATION_TOKEN_INVALID");
        }

        if (!token.User.IsActive)
        {
            throw new ForbiddenException("This account is inactive.", "USER_INACTIVE");
        }

        token.UsedAt = utcNow;
        token.User.IsEmailVerified = true;
        token.User.EmailVerifiedAt = utcNow;
        token.User.UpdatedAt = utcNow;

        var otherTokens = await dbContext.EmailVerificationTokens
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

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogService.WriteAsync(
            token.User.Id,
            AuditActions.EmailVerified,
            nameof(User),
            token.User.Id.ToString(),
            ipAddress,
            "User email verified.",
            cancellationToken);

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
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await dbContext.Users
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
        var url = _options.VerificationUrlTemplate.Replace(
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
