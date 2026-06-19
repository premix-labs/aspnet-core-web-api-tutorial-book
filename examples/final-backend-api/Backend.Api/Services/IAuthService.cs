using Backend.Api.Dtos.Auth;

namespace Backend.Api.Services;

public interface IAuthService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);

    Task RevokeRefreshTokenAsync(RevokeRefreshTokenRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<AuthSessionResponse>> GetSessionsAsync(CancellationToken cancellationToken);

    Task RevokeSessionAsync(Guid familyId, CancellationToken cancellationToken);

    Task RevokeAllSessionsAsync(CancellationToken cancellationToken);

    Task<CurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken);

    Task<CurrentUserResponse> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken);

    Task ResendEmailVerificationAsync(ResendEmailVerificationRequest request, CancellationToken cancellationToken);

    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);

    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
}
