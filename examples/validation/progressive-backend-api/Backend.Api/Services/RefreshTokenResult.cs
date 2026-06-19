namespace Backend.Api.Services;

public record RefreshTokenResult(string Token, DateTimeOffset ExpiresAt);
