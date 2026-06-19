namespace Backend.Api.Exceptions;

public class RefreshTokenReuseDetectedException(Guid userId)
    : UnauthorizedException("Refresh token reuse was detected.", "REFRESH_TOKEN_REUSE_DETECTED")
{
    public Guid UserId { get; } = userId;
}
