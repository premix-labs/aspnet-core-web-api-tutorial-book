namespace Backend.Api.Constants;

public static class AuditActions
{
    public const string UserRegistered = "USER_REGISTERED";
    public const string LoginSucceeded = "LOGIN_SUCCEEDED";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string AccountLocked = "ACCOUNT_LOCKED";
    public const string RefreshTokenRotated = "REFRESH_TOKEN_ROTATED";
    public const string RefreshTokenRevoked = "REFRESH_TOKEN_REVOKED";
    public const string RefreshTokenReuseDetected = "REFRESH_TOKEN_REUSE_DETECTED";
    public const string RefreshTokenSessionRevoked = "REFRESH_TOKEN_SESSION_REVOKED";
    public const string AllRefreshTokenSessionsRevoked = "ALL_REFRESH_TOKEN_SESSIONS_REVOKED";
    public const string EmailVerificationRequested = "EMAIL_VERIFICATION_REQUESTED";
    public const string EmailVerified = "EMAIL_VERIFIED";
    public const string PasswordResetRequested = "PASSWORD_RESET_REQUESTED";
    public const string PasswordResetCompleted = "PASSWORD_RESET_COMPLETED";
    public const string UserRoleChanged = "USER_ROLE_CHANGED";
    public const string UserStatusChanged = "USER_STATUS_CHANGED";
}
