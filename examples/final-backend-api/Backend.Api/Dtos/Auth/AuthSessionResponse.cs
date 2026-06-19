namespace Backend.Api.Dtos.Auth;

public record AuthSessionResponse(
    Guid FamilyId,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastIssuedAt,
    DateTimeOffset ExpiresAt,
    string? CreatedByIp,
    string? LastUsedByIp,
    string? UserAgent);