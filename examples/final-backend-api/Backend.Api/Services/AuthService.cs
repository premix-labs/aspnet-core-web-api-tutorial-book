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
    IJwtTokenService jwtTokenService,
    IRefreshTokenService refreshTokenService,
    IEmailVerificationService emailVerificationService,
    IPasswordResetService passwordResetService,
    ICurrentUserService currentUserService,
    IAuditLogService auditLogService,
    IHttpContextAccessor httpContextAccessor,
    IOptions<AccountLockoutOptions> lockoutOptions) : IAuthService
{
    private readonly AccountLockoutOptions _lockoutOptions = lockoutOptions.Value;

    public async Task<LoginResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var normalizedEmail = NormalizeEmail(email);

        if (await userRepository.NormalizedEmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new ConflictException("Email is already registered.", "EMAIL_ALREADY_EXISTS");
        }

        var user = new User
        {
            Email = email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = string.Empty,
            Role = Roles.User,
            IsActive = true,
            IsEmailVerified = false,
            PasswordChangedAt = DateTimeOffset.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        userRepository.Add(user);
        var refreshToken = await refreshTokenService.CreateAsync(
            user,
            GetIpAddress(),
            GetUserAgent(),
            cancellationToken);

        await userRepository.SaveChangesAsync(cancellationToken);
        await emailVerificationService.SendVerificationAsync(user, GetIpAddress(), cancellationToken);
        await auditLogService.WriteAsync(
            user.Id,
            AuditActions.UserRegistered,
            nameof(User),
            user.Id.ToString(),
            GetIpAddress(),
            "User registered with default role.",
            cancellationToken);

        return CreateLoginResponse(user, refreshToken);
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await userRepository.GetByNormalizedEmailAsync(email, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid email or password.", "INVALID_CREDENTIALS");
        }

        var utcNow = DateTimeOffset.UtcNow;

        if (user.LockoutEnd is not null && user.LockoutEnd > utcNow)
        {
            await auditLogService.WriteAsync(
                user.Id,
                AuditActions.LoginFailed,
                nameof(User),
                user.Id.ToString(),
                GetIpAddress(),
                "Login rejected because account is locked.",
                cancellationToken);

            throw new ForbiddenException("This account is temporarily locked.", "USER_LOCKED");
        }

        var passwordResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            await RecordFailedLoginAsync(user, utcNow, cancellationToken);
            throw new UnauthorizedException("Invalid email or password.", "INVALID_CREDENTIALS");
        }

        if (!user.IsActive)
        {
            await auditLogService.WriteAsync(
                user.Id,
                AuditActions.LoginFailed,
                nameof(User),
                user.Id.ToString(),
                GetIpAddress(),
                "Login rejected because account is inactive.",
                cancellationToken);

            throw new ForbiddenException("This account is inactive.", "USER_INACTIVE");
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = utcNow;

        if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            user.PasswordChangedAt = utcNow;
        }

        user.UpdatedAt = utcNow;

        var refreshToken = await refreshTokenService.CreateAsync(
            user,
            GetIpAddress(),
            GetUserAgent(),
            cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);
        await auditLogService.WriteAsync(
            user.Id,
            AuditActions.LoginSucceeded,
            nameof(User),
            user.Id.ToString(),
            GetIpAddress(),
            "User logged in successfully.",
            cancellationToken);

        return CreateLoginResponse(user, refreshToken);
    }

    public async Task<LoginResponse> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        User user;
        RefreshTokenResult refreshToken;

        try
        {
            (user, refreshToken) = await refreshTokenService.RotateAsync(
                request.RefreshToken,
                GetIpAddress(),
                GetUserAgent(),
                cancellationToken);
        }
        catch (RefreshTokenReuseDetectedException exception)
        {
            await auditLogService.WriteAsync(
                exception.UserId,
                AuditActions.RefreshTokenReuseDetected,
                nameof(User),
                exception.UserId.ToString(),
                GetIpAddress(),
                "Refresh token reuse detected; active token family revoked.",
                cancellationToken);

            throw;
        }

        await auditLogService.WriteAsync(
            user.Id,
            AuditActions.RefreshTokenRotated,
            nameof(User),
            user.Id.ToString(),
            GetIpAddress(),
            "Refresh token rotated.",
            cancellationToken);

        return CreateLoginResponse(user, refreshToken);
    }

    public async Task RevokeRefreshTokenAsync(
        RevokeRefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await refreshTokenService.RevokeAsync(
            request.RefreshToken,
            GetIpAddress(),
            cancellationToken);
        await auditLogService.WriteAsync(
            currentUserService.UserId,
            AuditActions.RefreshTokenRevoked,
            nameof(RefreshToken),
            "self",
            GetIpAddress(),
            "Refresh token revoke requested.",
            cancellationToken);
    }

    public Task<CurrentUserResponse> VerifyEmailAsync(
        VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        return emailVerificationService.VerifyAsync(request, GetIpAddress(), cancellationToken);
    }

    public Task ResendEmailVerificationAsync(
        ResendEmailVerificationRequest request,
        CancellationToken cancellationToken)
    {
        return emailVerificationService.ResendAsync(
            request,
            GetIpAddress(),
            cancellationToken);
    }

    public Task ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        return passwordResetService.ForgotPasswordAsync(
            request,
            GetIpAddress(),
            cancellationToken);
    }

    public Task ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        return passwordResetService.ResetPasswordAsync(
            request,
            GetIpAddress(),
            cancellationToken);
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            throw new UnauthorizedException("Missing user identity.", "INVALID_TOKEN");
        }

        var user = await userRepository.GetByIdAsync(userId.Value, cancellationToken)
            ?? throw new UnauthorizedException("User no longer exists.", "INVALID_TOKEN");

        if (!user.IsActive)
        {
            throw new ForbiddenException("This account is inactive.", "USER_INACTIVE");
        }

        return ToCurrentUserResponse(user);
    }

    public async Task<IReadOnlyList<AuthSessionResponse>> GetSessionsAsync(CancellationToken cancellationToken)
    {
        var user = await GetCurrentActiveUserAsync(cancellationToken);

        return await refreshTokenService.GetActiveSessionsAsync(user.Id, cancellationToken);
    }

    public async Task RevokeSessionAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var user = await GetCurrentActiveUserAsync(cancellationToken);
        var revoked = await refreshTokenService.RevokeSessionAsync(
            user.Id,
            familyId,
            GetIpAddress(),
            "SessionRevoked",
            cancellationToken);

        if (!revoked)
        {
            return;
        }

        await auditLogService.WriteAsync(
            user.Id,
            AuditActions.RefreshTokenSessionRevoked,
            nameof(RefreshToken),
            familyId.ToString(),
            GetIpAddress(),
            "Refresh token session revoked.",
            cancellationToken);
    }

    public async Task RevokeAllSessionsAsync(CancellationToken cancellationToken)
    {
        var user = await GetCurrentActiveUserAsync(cancellationToken);
        var revokedCount = await refreshTokenService.RevokeAllSessionsAsync(
            user.Id,
            GetIpAddress(),
            "AllSessionsRevoked",
            cancellationToken);

        if (revokedCount == 0)
        {
            return;
        }

        await auditLogService.WriteAsync(
            user.Id,
            AuditActions.AllRefreshTokenSessionsRevoked,
            nameof(RefreshToken),
            user.Id.ToString(),
            GetIpAddress(),
            "All refresh token sessions revoked.",
            cancellationToken);
    }

    private LoginResponse CreateLoginResponse(User user, RefreshTokenResult refreshToken)
    {
        var token = jwtTokenService.CreateToken(user);

        return new LoginResponse(
            token.AccessToken,
            token.ExpiresAt,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            ToCurrentUserResponse(user));
    }

    private static CurrentUserResponse ToCurrentUserResponse(User user)
    {
        return new CurrentUserResponse(
            user.Id,
            user.Email,
            user.Role,
            user.IsActive,
            user.IsEmailVerified);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
    }

    private async Task RecordFailedLoginAsync(
        User user,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        user.AccessFailedCount++;

        if (user.AccessFailedCount >= _lockoutOptions.MaxFailedAccessAttempts)
        {
            user.LockoutEnd = utcNow.AddMinutes(_lockoutOptions.LockoutMinutes);
        }

        user.UpdatedAt = utcNow;

        await userRepository.SaveChangesAsync(cancellationToken);

        await auditLogService.WriteAsync(
            user.Id,
            user.LockoutEnd is null ? AuditActions.LoginFailed : AuditActions.AccountLocked,
            nameof(User),
            user.Id.ToString(),
            GetIpAddress(),
            user.LockoutEnd is null
                ? "Login failed because password was invalid."
                : "Account locked after repeated failed login attempts.",
            cancellationToken);
    }

    private string? GetIpAddress()
    {
        return httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string? GetUserAgent()
    {
        return httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
    }

    private async Task<User> GetCurrentActiveUserAsync(CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            throw new UnauthorizedException("Missing user identity.", "INVALID_TOKEN");
        }

        var user = await userRepository.GetByIdAsync(userId.Value, cancellationToken)
            ?? throw new UnauthorizedException("User no longer exists.", "INVALID_TOKEN");

        if (!user.IsActive)
        {
            throw new ForbiddenException("This account is inactive.", "USER_INACTIVE");
        }

        return user;
    }
}
