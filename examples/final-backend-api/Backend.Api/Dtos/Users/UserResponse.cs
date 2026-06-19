namespace Backend.Api.Dtos.Users;

public record UserResponse(
    Guid Id,
    string Email,
    string Role,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
