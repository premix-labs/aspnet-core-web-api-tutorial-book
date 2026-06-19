using Microsoft.EntityFrameworkCore;
using Backend.Api.Constants;
using Backend.Api.Dtos.Admin;
using Backend.Api.Dtos.Common;
using Backend.Api.Exceptions;
using Backend.Api.Models;
using Backend.Api.Repositories;

namespace Backend.Api.Services;

public class AdminUserService(
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IAuditLogService auditLogService) : IAdminUserService
{
    public async Task<PagedResponse<AdminUserResponse>> QueryUsersAsync(
        AdminUserQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var users = userRepository.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            users = users.Where(user => user.NormalizedEmail.Contains(search.ToUpperInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(query.Role) && Roles.IsValid(query.Role))
        {
            var role = Roles.Normalize(query.Role);
            users = users.Where(user => user.Role == role);
        }

        if (query.IsActive is not null)
        {
            users = users.Where(user => user.IsActive == query.IsActive.Value);
        }

        users = ApplySorting(users, query.SortBy, query.SortDirection);

        var totalItems = await users.CountAsync(cancellationToken);
        var items = await users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new AdminUserResponse(
                user.Id,
                user.Email,
                user.Role,
                user.IsActive,
                user.IsEmailVerified,
                user.AccessFailedCount,
                user.LockoutEnd,
                user.LastLoginAt,
                user.CreatedAt,
                user.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResponse<AdminUserResponse>(items, page, pageSize, totalItems);
    }

    public async Task<AdminUserResponse> GetUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await userRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
            ?? throw new NotFoundException("User was not found.", "USER_NOT_FOUND");

        return ToResponse(user);
    }

    public async Task<AdminUserResponse> ChangeRoleAsync(
        Guid id,
        ChangeUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User was not found.", "USER_NOT_FOUND");

        var newRole = Roles.Normalize(request.Role);

        if (user.Id == currentUserService.UserId && newRole != Roles.Admin)
        {
            throw new ConflictException(
                "You cannot remove your own admin role.",
                "ADMIN_SELF_DEMOTE_NOT_ALLOWED");
        }

        if (user.Role == Roles.Admin && newRole != Roles.Admin)
        {
            await EnsureAtLeastOneActiveAdminRemainsAsync(cancellationToken);
        }

        var oldRole = user.Role;
        user.Role = newRole;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await userRepository.SaveChangesAsync(cancellationToken);
        await auditLogService.WriteAsync(
            currentUserService.UserId,
            AuditActions.UserRoleChanged,
            nameof(User),
            user.Id.ToString(),
            null,
            $"Role changed from {oldRole} to {newRole}.",
            cancellationToken);

        return ToResponse(user);
    }

    public async Task<AdminUserResponse> ChangeStatusAsync(
        Guid id,
        ChangeUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User was not found.", "USER_NOT_FOUND");

        if (user.Id == currentUserService.UserId && !request.IsActive)
        {
            throw new ConflictException(
                "You cannot deactivate your own account.",
                "ADMIN_SELF_DEACTIVATE_NOT_ALLOWED");
        }

        if (user.Role == Roles.Admin && user.IsActive && !request.IsActive)
        {
            await EnsureAtLeastOneActiveAdminRemainsAsync(cancellationToken);
        }

        var oldStatus = user.IsActive;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await userRepository.SaveChangesAsync(cancellationToken);
        await auditLogService.WriteAsync(
            currentUserService.UserId,
            AuditActions.UserStatusChanged,
            nameof(User),
            user.Id.ToString(),
            null,
            $"IsActive changed from {oldStatus} to {request.IsActive}.",
            cancellationToken);

        return ToResponse(user);
    }

    private async Task EnsureAtLeastOneActiveAdminRemainsAsync(CancellationToken cancellationToken)
    {
        var activeAdminCount = await userRepository.CountActiveAdminsAsync(cancellationToken);

        if (activeAdminCount <= 1)
        {
            throw new ConflictException(
                "At least one active admin must remain.",
                "LAST_ACTIVE_ADMIN_REQUIRED");
        }
    }

    private static IQueryable<User> ApplySorting(
        IQueryable<User> users,
        string sortBy,
        string sortDirection)
    {
        var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy.Trim().ToLowerInvariant(), descending) switch
        {
            ("email", true) => users.OrderByDescending(user => user.Email),
            ("email", false) => users.OrderBy(user => user.Email),
            ("role", true) => users.OrderByDescending(user => user.Role),
            ("role", false) => users.OrderBy(user => user.Role),
            ("isactive", true) => users.OrderByDescending(user => user.IsActive),
            ("isactive", false) => users.OrderBy(user => user.IsActive),
            ("createdat", false) => users.OrderBy(user => user.CreatedAt),
            _ => users.OrderByDescending(user => user.CreatedAt)
        };
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
