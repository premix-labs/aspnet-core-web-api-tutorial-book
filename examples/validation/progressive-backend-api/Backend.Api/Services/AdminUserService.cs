using Backend.Api.Constants;
using Backend.Api.Dtos.Admin;
using Backend.Api.Dtos.Common;
using Backend.Api.Exceptions;
using Backend.Api.Models;
using Backend.Api.Repositories;

namespace Backend.Api.Services;

public class AdminUserService(
    IUserRepository userRepository,
    CurrentUserService currentUserService,
    AuditLogService auditLogService,
    ILogger<AdminUserService> logger)
{
    public async Task<PagedResponse<AdminUserResponse>> GetUsersAsync(
        AdminUserQuery query)
    {
        var result = await userRepository.QueryUsersAsync(query);

        return new PagedResponse<AdminUserResponse>(
            result.Items.Select(ToResponse).ToList(),
            result.Page,
            result.PageSize,
            result.TotalItems);
    }

    public async Task<AdminUserResponse> UpdateRoleAsync(
        Guid id,
        UpdateUserRoleRequest request)
    {
        var currentAdminId = GetCurrentAdminId();
        var user = await userRepository.GetByIdAsync(id);

        if (user is null)
        {
            throw new NotFoundException("User not found", "USER_NOT_FOUND");
        }

        var nextRole = Roles.Normalize(request.Role);

        if (user.Id == currentAdminId && nextRole != Roles.Admin)
        {
            throw new ForbiddenException(
                "Admin cannot demote own account",
                "ADMIN_SELF_DEMOTE_NOT_ALLOWED");
        }

        var activeAdminCount = await userRepository.CountActiveAdminsAsync();

        if (user.Role == Roles.Admin &&
            nextRole != Roles.Admin &&
            user.IsActive &&
            activeAdminCount <= 1)
        {
            throw new ForbiddenException(
                "Cannot remove the last active admin",
                "LAST_ACTIVE_ADMIN_REQUIRED");
        }

        var oldRole = user.Role;
        user.Role = nextRole;

        await userRepository.UpdateAsync(user);

        await auditLogService.LogAsync(
            currentAdminId,
            AuditActions.UserRoleChanged,
            nameof(User),
            user.Id.ToString(),
            null,
            $"Role changed from {oldRole} to {user.Role}");

        logger.LogInformation(
            "Admin {AdminUserId} changed user {TargetUserId} role from {OldRole} to {NewRole}",
            currentAdminId,
            user.Id,
            oldRole,
            user.Role);

        return ToResponse(user);
    }

    public async Task<AdminUserResponse> UpdateStatusAsync(
        Guid id,
        UpdateUserStatusRequest request)
    {
        var currentAdminId = GetCurrentAdminId();
        var user = await userRepository.GetByIdAsync(id);

        if (user is null)
        {
            throw new NotFoundException("User not found", "USER_NOT_FOUND");
        }

        var nextIsActive = request.IsActive!.Value;

        if (user.Id == currentAdminId && !nextIsActive)
        {
            throw new ForbiddenException(
                "Admin cannot deactivate own account",
                "ADMIN_SELF_DEACTIVATE_NOT_ALLOWED");
        }

        var activeAdminCount = await userRepository.CountActiveAdminsAsync();

        if (user.Role == Roles.Admin &&
            user.IsActive &&
            !nextIsActive &&
            activeAdminCount <= 1)
        {
            throw new ForbiddenException(
                "Cannot deactivate the last active admin",
                "LAST_ACTIVE_ADMIN_REQUIRED");
        }

        var oldIsActive = user.IsActive;
        user.IsActive = nextIsActive;

        await userRepository.UpdateAsync(user);

        await auditLogService.LogAsync(
            currentAdminId,
            AuditActions.UserStatusChanged,
            nameof(User),
            user.Id.ToString(),
            null,
            $"IsActive changed from {oldIsActive} to {user.IsActive}");

        logger.LogInformation(
            "Admin {AdminUserId} changed user {TargetUserId} active status from {OldStatus} to {NewStatus}",
            currentAdminId,
            user.Id,
            oldIsActive,
            user.IsActive);

        return ToResponse(user);
    }

    private Guid GetCurrentAdminId()
    {
        if (currentUserService.UserId is null)
        {
            throw new UnauthorizedException("Invalid token", "INVALID_TOKEN");
        }

        return currentUserService.UserId.Value;
    }

    private static AdminUserResponse ToResponse(User user)
    {
        return new AdminUserResponse(
            user.Id,
            user.Email,
            user.Role,
            user.IsActive,
            user.IsEmailVerified,
            user.AccessFailedCount,
            user.LockoutEnd,
            user.LastLoginAt,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
