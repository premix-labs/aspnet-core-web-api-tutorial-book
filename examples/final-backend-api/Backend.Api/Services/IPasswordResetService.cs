using Backend.Api.Dtos.Auth;

namespace Backend.Api.Services;

public interface IPasswordResetService
{
    Task ForgotPasswordAsync(
        ForgotPasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task ResetPasswordAsync(
        ResetPasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken);
}
