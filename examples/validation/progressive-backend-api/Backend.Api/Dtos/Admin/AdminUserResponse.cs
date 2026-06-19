namespace Backend.Api.Dtos.Admin;

public record AdminUserResponse(
    Guid Id,
    string Email,
    string Role,
    bool IsActive,
    bool IsEmailVerified,
    int AccessFailedCount,
    DateTimeOffset? LockoutEnd,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
