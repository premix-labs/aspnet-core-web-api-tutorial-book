using Backend.Api.Dtos.Admin;
using Backend.Api.Dtos.Common;
using Backend.Api.Models;

namespace Backend.Api.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();

    Task<User?> GetByIdAsync(Guid id);

    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByNormalizedEmailAsync(string normalizedEmail);

    Task<bool> NormalizedEmailExistsAsync(string normalizedEmail);

    Task<User> CreateAsync(User user);

    Task<bool> UpdateAsync(User user);

    Task<bool> DeleteAsync(User user);

    Task<int> CountActiveAdminsAsync();

    Task<PagedResponse<User>> QueryUsersAsync(AdminUserQuery query);
}
