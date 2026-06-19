namespace Backend.Api.Dtos.Auth;

public record CurrentUserResponse(
    Guid Id,
    string Email,
    string Role,
    bool IsActive,
    bool IsEmailVerified);
