using Backend.Api.Dtos.Admin;
using Backend.Api.Dtos.Common;

namespace Backend.Api.Services;

public interface IAdminUserService
{
    Task<PagedResponse<AdminUserResponse>> QueryUsersAsync(
        AdminUserQuery query,
        CancellationToken cancellationToken);

    Task<AdminUserResponse> GetUserAsync(Guid id, CancellationToken cancellationToken);

    Task<AdminUserResponse> ChangeRoleAsync(
        Guid id,
        ChangeUserRoleRequest request,
        CancellationToken cancellationToken);

    Task<AdminUserResponse> ChangeStatusAsync(
        Guid id,
        ChangeUserStatusRequest request,
        CancellationToken cancellationToken);
}
