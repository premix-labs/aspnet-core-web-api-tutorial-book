using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Backend.Api.Data;
using Backend.Api.Dtos.Auth;
using Backend.Api.Exceptions;
using Backend.Api.Models;
using Backend.Api.Options;

namespace Backend.Api.Services;

public class RefreshTokenService(
    AppDbContext dbContext,
    IOptions<RefreshTokenOptions> options) : IRefreshTokenService
{
    private readonly RefreshTokenOptions _options = options.Value;

    public Task<RefreshTokenResult> CreateAsync(
        User user,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        return CreateAsync(user, ipAddress, userAgent, Guid.NewGuid(), cancellationToken);
    }

    private Task<RefreshTokenResult> CreateAsync(
        User user,
        string? ipAddress,
        string? userAgent,
        Guid familyId,
        CancellationToken cancellationToken)
    {
        var token = GenerateToken();
        var tokenHash = HashToken(token);
        var expiresAt = DateTimeOffset.UtcNow.AddDays(_options.ExpirationDays);

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            FamilyId = familyId,
            TokenHash = tokenHash,
            CreatedByIp = ipAddress,
            UserAgent = userAgent,
            ExpiresAt = expiresAt
        });

        return Task.FromResult(new RefreshTokenResult(token, expiresAt));
    }

    public async Task<(User User, RefreshTokenResult RefreshToken)> RotateAsync(
        string refreshToken,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var token = await FindActiveTokenAsync(refreshToken, ipAddress, cancellationToken);
        var newRefreshToken = await CreateAsync(
            token.User,
            ipAddress,
            userAgent ?? token.UserAgent,
            token.FamilyId,
            cancellationToken);

        token.RevokedAt = DateTimeOffset.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReplacedByTokenHash = HashToken(newRefreshToken.Token);
        token.RevocationReason = "Rotated";

        await dbContext.SaveChangesAsync(cancellationToken);

        return (token.User, newRefreshToken);
    }

    public async Task RevokeAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(refreshToken);
        var token = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (token is null || token.RevokedAt is not null)
        {
            return;
        }

        token.RevokedAt = DateTimeOffset.UtcNow;
        token.RevokedByIp = ipAddress;
        token.RevocationReason = "UserRevoked";

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllForUserAsync(
        Guid userId,
        string? ipAddress,
        string? reason,
        CancellationToken cancellationToken)
    {
        await RevokeAllSessionsAsync(userId, ipAddress, reason, cancellationToken);
    }

    public async Task<IReadOnlyList<AuthSessionResponse>> GetActiveSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var activeTokens = await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(token =>
                token.UserId == userId &&
                token.RevokedAt == null &&
                token.ExpiresAt > utcNow)
            .ToListAsync(cancellationToken);

        return activeTokens
            .GroupBy(token => token.FamilyId)
            .Select(group =>
            {
                var firstToken = group.OrderBy(token => token.CreatedAt).First();
                var latestToken = group.OrderByDescending(token => token.CreatedAt).First();

                return new AuthSessionResponse(
                    group.Key,
                    firstToken.CreatedAt,
                    latestToken.CreatedAt,
                    latestToken.ExpiresAt,
                    firstToken.CreatedByIp,
                    latestToken.CreatedByIp,
                    latestToken.UserAgent ?? firstToken.UserAgent);
            })
            .OrderByDescending(session => session.LastIssuedAt)
            .ToList();
    }

    public async Task<bool> RevokeSessionAsync(
        Guid userId,
        Guid familyId,
        string? ipAddress,
        string? reason,
        CancellationToken cancellationToken)
    {
        var revokedCount = await RevokeActiveTokensAsync(
            token => token.UserId == userId && token.FamilyId == familyId,
            ipAddress,
            reason ?? "SessionRevoked",
            cancellationToken);

        return revokedCount > 0;
    }

    public async Task<int> RevokeAllSessionsAsync(
        Guid userId,
        string? ipAddress,
        string? reason,
        CancellationToken cancellationToken)
    {
        return await RevokeActiveTokensAsync(
            token => token.UserId == userId,
            ipAddress,
            reason ?? "AllSessionsRevoked",
            cancellationToken);
    }

    private async Task<int> RevokeActiveTokensAsync(
        System.Linq.Expressions.Expression<Func<RefreshToken, bool>> predicate,
        string? ipAddress,
        string reason,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var activeTokens = await dbContext.RefreshTokens
            .Where(predicate)
            .Where(token => token.RevokedAt == null && token.ExpiresAt > utcNow)
            .ToListAsync(cancellationToken);

        foreach (var activeToken in activeTokens)
        {
            activeToken.RevokedAt = utcNow;
            activeToken.RevokedByIp = ipAddress;
            activeToken.RevocationReason = reason;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return activeTokens.Count;
    }

    private async Task<RefreshToken> FindActiveTokenAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(refreshToken);
        var token = await dbContext.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        var utcNow = DateTimeOffset.UtcNow;

        if (token is null)
        {
            throw new UnauthorizedException(
                "Refresh token is invalid or expired.",
                "REFRESH_TOKEN_INVALID");
        }

        if (token.RevokedAt is not null && token.ReplacedByTokenHash is not null)
        {
            await RevokeTokenFamilyAsync(
                token.UserId,
                token.FamilyId,
                ipAddress,
                "ReuseDetected",
                cancellationToken);

            throw new RefreshTokenReuseDetectedException(token.UserId);
        }

        if (!token.IsActive(utcNow))
        {
            throw new UnauthorizedException(
                "Refresh token is invalid or expired.",
                "REFRESH_TOKEN_INVALID");
        }

        if (!token.User.IsActive)
        {
            throw new ForbiddenException("This account is inactive.", "USER_INACTIVE");
        }

        return token;
    }

    private async Task RevokeTokenFamilyAsync(
        Guid userId,
        Guid familyId,
        string? ipAddress,
        string reason,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var activeTokens = await dbContext.RefreshTokens
            .Where(token =>
                token.UserId == userId &&
                token.FamilyId == familyId &&
                token.RevokedAt == null &&
                token.ExpiresAt > utcNow)
            .ToListAsync(cancellationToken);

        foreach (var activeToken in activeTokens)
        {
            activeToken.RevokedAt = utcNow;
            activeToken.RevokedByIp = ipAddress;
            activeToken.RevocationReason = reason;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
}
