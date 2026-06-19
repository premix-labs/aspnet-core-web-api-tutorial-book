namespace Backend.Api.Services;

public record TokenResult(string AccessToken, DateTimeOffset ExpiresAt);
