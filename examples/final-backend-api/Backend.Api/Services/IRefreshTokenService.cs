using Backend.Api.Dtos.Auth;
using Backend.Api.Models;

namespace Backend.Api.Services;

public interface IRefreshTokenService
{
    Task<RefreshTokenResult> CreateAsync(
        User user,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);

    Task<(User User, RefreshTokenResult RefreshToken)> RotateAsync(
        string refreshToken,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);

    Task RevokeAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task RevokeAllForUserAsync(
        Guid userId,
        string? ipAddress,
        string? reason,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AuthSessionResponse>> GetActiveSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<bool> RevokeSessionAsync(
        Guid userId,
        Guid familyId,
        string? ipAddress,
        string? reason,
        CancellationToken cancellationToken);

    Task<int> RevokeAllSessionsAsync(
        Guid userId,
        string? ipAddress,
        string? reason,
        CancellationToken cancellationToken);
}
