using Backend.Api.Dtos.Users;

namespace Backend.Api.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetUsersAsync();

    Task<UserResponse> GetUserByIdAsync(Guid id);

    Task<UserResponse> CreateUserAsync(CreateUserRequest request);

    Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request);

    Task DeleteUserAsync(Guid id);
}
