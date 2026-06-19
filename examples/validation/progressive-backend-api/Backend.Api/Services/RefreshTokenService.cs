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
    AppDbContext db,
    IOptions<RefreshTokenOptions> refreshTokenOptions)
{
    public async Task<RefreshTokenResult> CreateAsync(
        User user,
        string? ipAddress,
        string? userAgent)
    {
        return await CreateAsync(user, ipAddress, userAgent, Guid.NewGuid());
    }

    private async Task<RefreshTokenResult> CreateAsync(
        User user,
        string? ipAddress,
        string? userAgent,
        Guid familyId)
    {
        var token = GenerateToken();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(refreshTokenOptions.Value.ExpirationDays);

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            FamilyId = familyId,
            TokenHash = HashToken(token),
            CreatedByIp = ipAddress,
            UserAgent = userAgent,
            ExpiresAt = expiresAt
        });

        await db.SaveChangesAsync();

        return new RefreshTokenResult(token, expiresAt);
    }

    public async Task<(User User, RefreshTokenResult RefreshToken)> RotateAsync(
        string refreshToken,
        string? ipAddress,
        string? userAgent)
    {
        var storedToken = await FindActiveTokenAsync(refreshToken, ipAddress);
        var newRefreshToken = await CreateAsync(
            storedToken.User,
            ipAddress,
            userAgent ?? storedToken.UserAgent,
            storedToken.FamilyId);

        storedToken.RevokedAt = DateTimeOffset.UtcNow;
        storedToken.RevokedByIp = ipAddress;
        storedToken.ReplacedByTokenHash = HashToken(newRefreshToken.Token);
        storedToken.RevocationReason = "Rotated";

        await db.SaveChangesAsync();

        return (storedToken.User, newRefreshToken);
    }

    public async Task RevokeAsync(string refreshToken, string? ipAddress)
    {
        var tokenHash = HashToken(refreshToken);
        var storedToken = await db.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash);

        if (storedToken is null || storedToken.RevokedAt is not null)
        {
            return;
        }

        storedToken.RevokedAt = DateTimeOffset.UtcNow;
        storedToken.RevokedByIp = ipAddress;
        storedToken.RevocationReason = "UserRevoked";

        await db.SaveChangesAsync();
    }

    public async Task RevokeAllForUserAsync(
        Guid userId,
        string? ipAddress,
        string? reason = null)
    {
        await RevokeAllSessionsAsync(userId, ipAddress, reason);
    }

    public async Task<IReadOnlyList<AuthSessionResponse>> GetActiveSessionsAsync(Guid userId)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var activeTokens = await db.RefreshTokens
            .AsNoTracking()
            .Where(token =>
                token.UserId == userId &&
                token.RevokedAt == null &&
                token.ExpiresAt > utcNow)
            .ToListAsync();

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
        string? reason)
    {
        var revokedCount = await RevokeActiveTokensAsync(
            token => token.UserId == userId && token.FamilyId == familyId,
            ipAddress,
            reason ?? "SessionRevoked");

        return revokedCount > 0;
    }

    public async Task<int> RevokeAllSessionsAsync(
        Guid userId,
        string? ipAddress,
        string? reason)
    {
        return await RevokeActiveTokensAsync(
            token => token.UserId == userId,
            ipAddress,
            reason ?? "AllSessionsRevoked");
    }

    private async Task<int> RevokeActiveTokensAsync(
        System.Linq.Expressions.Expression<Func<RefreshToken, bool>> predicate,
        string? ipAddress,
        string reason)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var activeTokens = await db.RefreshTokens
            .Where(predicate)
            .Where(token => token.RevokedAt == null && token.ExpiresAt > utcNow)
            .ToListAsync();

        foreach (var activeToken in activeTokens)
        {
            activeToken.RevokedAt = utcNow;
            activeToken.RevokedByIp = ipAddress;
            activeToken.RevocationReason = reason;
        }

        await db.SaveChangesAsync();

        return activeTokens.Count;
    }

    private async Task<RefreshToken> FindActiveTokenAsync(
        string refreshToken,
        string? ipAddress)
    {
        var tokenHash = HashToken(refreshToken);
        var storedToken = await db.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash);

        if (storedToken is null)
        {
            throw new UnauthorizedException(
                "Refresh token is invalid or expired",
                "REFRESH_TOKEN_INVALID");
        }

        if (storedToken.RevokedAt is not null && storedToken.ReplacedByTokenHash is not null)
        {
            await RevokeTokenFamilyAsync(
                storedToken.UserId,
                storedToken.FamilyId,
                ipAddress,
                "ReuseDetected");

            throw new RefreshTokenReuseDetectedException(storedToken.UserId);
        }

        if (!storedToken.IsActive(DateTimeOffset.UtcNow))
        {
            throw new UnauthorizedException(
                "Refresh token is invalid or expired",
                "REFRESH_TOKEN_INVALID");
        }

        if (!storedToken.User.IsActive)
        {
            throw new ForbiddenException(
                "User account is inactive",
                "USER_INACTIVE");
        }

        return storedToken;
    }

    private async Task RevokeTokenFamilyAsync(
        Guid userId,
        Guid familyId,
        string? ipAddress,
        string reason)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var activeTokens = await db.RefreshTokens
            .Where(token =>
                token.UserId == userId &&
                token.FamilyId == familyId &&
                token.RevokedAt == null &&
                token.ExpiresAt > utcNow)
            .ToListAsync();

        foreach (var activeToken in activeTokens)
        {
            activeToken.RevokedAt = utcNow;
            activeToken.RevokedByIp = ipAddress;
            activeToken.RevocationReason = reason;
        }

        await db.SaveChangesAsync();
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
