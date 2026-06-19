using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Backend.Api.Constants;
using Backend.Api.Dtos.Auth;
using Backend.Api.Exceptions;
using Backend.Api.Models;
using Backend.Api.Options;
using Backend.Api.Repositories;

namespace Backend.Api.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    JwtTokenService jwtTokenService,
    RefreshTokenService refreshTokenService,
    CurrentUserService currentUserService,
    EmailVerificationService emailVerificationService,
    PasswordResetService passwordResetService,
    AuditLogService auditLogService,
    IHttpContextAccessor httpContextAccessor,
    IOptions<AccountLockoutOptions> lockoutOptions,
    ILogger<AuthService> logger)
{
    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        logger.LogInformation("Register requested for email {Email}", request.Email);

        var email = request.Email.Trim();
        var normalizedEmail = NormalizeEmail(email);
        var existingUser = await userRepository.GetByNormalizedEmailAsync(normalizedEmail);

        if (existingUser is not null)
        {
            logger.LogWarning(
                "Register failed for email {Email}: email already exists",
                request.Email);

            throw new ConflictException("Email already exists", "EMAIL_ALREADY_EXISTS");
        }

        var user = new User
        {
            Email = email,
            NormalizedEmail = normalizedEmail,
            Role = Roles.User,
            IsActive = true,
            IsEmailVerified = false,
            PasswordChangedAt = DateTimeOffset.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        var createdUser = await userRepository.CreateAsync(user);
        var refreshToken = await refreshTokenService.CreateAsync(
            createdUser,
            GetIpAddress(),
            GetUserAgent());
        await emailVerificationService.SendVerificationAsync(createdUser, GetIpAddress());
        await auditLogService.LogAsync(
            createdUser.Id,
            AuditActions.UserRegistered,
            nameof(User),
            createdUser.Id.ToString(),
            GetIpAddress(),
            "User registered with default role.");

        logger.LogInformation(
            "User {UserId} registered with role {Role}",
            createdUser.Id,
            createdUser.Role);

        return jwtTokenService.GenerateLoginResponse(createdUser, refreshToken);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByNormalizedEmailAsync(NormalizeEmail(request.Email));

        if (user is null)
        {
            logger.LogWarning(
                "Login failed for email {Email}: invalid credentials",
                request.Email);

            throw new UnauthorizedException(
                "Invalid email or password",
                "INVALID_CREDENTIALS");
        }

        var utcNow = DateTimeOffset.UtcNow;

        if (user.LockoutEnd is not null && user.LockoutEnd > utcNow)
        {
            await auditLogService.LogAsync(
                user.Id,
                AuditActions.LoginFailed,
                nameof(User),
                user.Id.ToString(),
                GetIpAddress(),
                "Login rejected because account is locked.");

            throw new ForbiddenException(
                "User account is temporarily locked",
                "USER_LOCKED");
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            await RecordFailedLoginAsync(user, utcNow);

            logger.LogWarning(
                "Login failed for email {Email}: invalid credentials",
                request.Email);

            throw new UnauthorizedException(
                "Invalid email or password",
                "INVALID_CREDENTIALS");
        }

        if (!user.IsActive)
        {
            await auditLogService.LogAsync(
                user.Id,
                AuditActions.LoginFailed,
                nameof(User),
                user.Id.ToString(),
                GetIpAddress(),
                "Login rejected because account is inactive.");

            logger.LogWarning(
                "Login failed for user {UserId}: inactive account",
                user.Id);

            throw new ForbiddenException(
                "User account is inactive",
                "USER_INACTIVE");
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = utcNow;
        user.UpdatedAt = utcNow;

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            user.PasswordChangedAt = utcNow;
        }

        await userRepository.UpdateAsync(user);
        var refreshToken = await refreshTokenService.CreateAsync(
            user,
            GetIpAddress(),
            GetUserAgent());
        await auditLogService.LogAsync(
            user.Id,
            AuditActions.LoginSucceeded,
            nameof(User),
            user.Id.ToString(),
            GetIpAddress(),
            "User logged in successfully.");

        logger.LogInformation("User {UserId} logged in", user.Id);

        return jwtTokenService.GenerateLoginResponse(user, refreshToken);
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request)
    {
        User user;
        RefreshTokenResult refreshToken;

        try
        {
            (user, refreshToken) = await refreshTokenService.RotateAsync(
                request.RefreshToken,
                GetIpAddress(),
                GetUserAgent());
        }
        catch (RefreshTokenReuseDetectedException exception)
        {
            await auditLogService.LogAsync(
                exception.UserId,
                AuditActions.RefreshTokenReuseDetected,
                nameof(User),
                exception.UserId.ToString(),
                GetIpAddress(),
                "Refresh token reuse detected; active token family revoked.");

            throw;
        }

        await auditLogService.LogAsync(
            user.Id,
            AuditActions.RefreshTokenRotated,
            nameof(User),
            user.Id.ToString(),
            GetIpAddress(),
            "Refresh token rotated.");

        return jwtTokenService.GenerateLoginResponse(user, refreshToken);
    }

    public async Task RevokeRefreshTokenAsync(RevokeRefreshTokenRequest request)
    {
        await refreshTokenService.RevokeAsync(request.RefreshToken, GetIpAddress());
        await auditLogService.LogAsync(
            currentUserService.UserId,
            AuditActions.RefreshTokenRevoked,
            nameof(RefreshToken),
            "self",
            GetIpAddress(),
            "Refresh token revoke requested.");
    }

    public Task<CurrentUserResponse> VerifyEmailAsync(VerifyEmailRequest request)
    {
        return emailVerificationService.VerifyAsync(request, GetIpAddress());
    }

    public Task ResendEmailVerificationAsync(ResendEmailVerificationRequest request)
    {
        return emailVerificationService.ResendAsync(request, GetIpAddress());
    }

    public Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        return passwordResetService.ForgotPasswordAsync(request, GetIpAddress());
    }

    public Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        return passwordResetService.ResetPasswordAsync(request, GetIpAddress());
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync()
    {
        if (currentUserService.UserId is null)
        {
            throw new UnauthorizedException("Invalid token", "INVALID_TOKEN");
        }

        var user = await userRepository.GetByIdAsync(currentUserService.UserId.Value);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid token", "INVALID_TOKEN");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException("User account is inactive", "USER_INACTIVE");
        }

        return new CurrentUserResponse(
            user.Id,
            user.Email,
            user.Role,
            user.IsActive,
            user.IsEmailVerified);
    }

    public async Task<IReadOnlyList<AuthSessionResponse>> GetSessionsAsync()
    {
        var user = await GetCurrentActiveUserAsync();

        return await refreshTokenService.GetActiveSessionsAsync(user.Id);
    }

    public async Task RevokeSessionAsync(Guid familyId)
    {
        var user = await GetCurrentActiveUserAsync();
        var revoked = await refreshTokenService.RevokeSessionAsync(
            user.Id,
            familyId,
            GetIpAddress(),
            "SessionRevoked");

        if (!revoked)
        {
            return;
        }

        await auditLogService.LogAsync(
            user.Id,
            AuditActions.RefreshTokenSessionRevoked,
            nameof(RefreshToken),
            familyId.ToString(),
            GetIpAddress(),
            "Refresh token session revoked.");
    }

    public async Task RevokeAllSessionsAsync()
    {
        var user = await GetCurrentActiveUserAsync();
        var revokedCount = await refreshTokenService.RevokeAllSessionsAsync(
            user.Id,
            GetIpAddress(),
            "AllSessionsRevoked");

        if (revokedCount == 0)
        {
            return;
        }

        await auditLogService.LogAsync(
            user.Id,
            AuditActions.AllRefreshTokenSessionsRevoked,
            nameof(RefreshToken),
            user.Id.ToString(),
            GetIpAddress(),
            "All refresh token sessions revoked.");
    }

    private async Task RecordFailedLoginAsync(User user, DateTimeOffset utcNow)
    {
        user.AccessFailedCount++;

        if (user.AccessFailedCount >= lockoutOptions.Value.MaxFailedAccessAttempts)
        {
            user.LockoutEnd = utcNow.AddMinutes(lockoutOptions.Value.LockoutMinutes);
        }

        user.UpdatedAt = utcNow;

        await userRepository.UpdateAsync(user);
        await auditLogService.LogAsync(
            user.Id,
            user.LockoutEnd is null ? AuditActions.LoginFailed : AuditActions.AccountLocked,
            nameof(User),
            user.Id.ToString(),
            GetIpAddress(),
            user.LockoutEnd is null
                ? "Login failed because password was invalid."
                : "Account locked after repeated failed login attempts.");
    }

    private string? GetIpAddress()
    {
        return httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string? GetUserAgent()
    {
        return httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
    }

    private async Task<User> GetCurrentActiveUserAsync()
    {
        if (currentUserService.UserId is null)
        {
            throw new UnauthorizedException("Invalid token", "INVALID_TOKEN");
        }

        var user = await userRepository.GetByIdAsync(currentUserService.UserId.Value);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid token", "INVALID_TOKEN");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException("User account is inactive", "USER_INACTIVE");
        }

        return user;
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
    }
}
