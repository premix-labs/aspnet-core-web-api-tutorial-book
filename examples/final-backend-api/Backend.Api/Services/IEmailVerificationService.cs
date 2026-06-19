using Backend.Api.Dtos.Auth;
using Backend.Api.Models;

namespace Backend.Api.Services;

public interface IEmailVerificationService
{
    Task SendVerificationAsync(
        User user,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task<CurrentUserResponse> VerifyAsync(
        VerifyEmailRequest request,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task ResendAsync(
        ResendEmailVerificationRequest request,
        string? ipAddress,
        CancellationToken cancellationToken);
}
