using Backend.Api.Dtos.Users;
using Backend.Api.Exceptions;
using Backend.Api.Models;
using Backend.Api.Repositories;

namespace Backend.Api.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<IReadOnlyList<UserResponse>> GetUsersAsync()
    {
        var users = await userRepository.GetAllAsync();

        return users
            .Select(ToResponse)
            .ToList();
    }

    public async Task<UserResponse> GetUserByIdAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);

        if (user is null)
        {
            throw new NotFoundException("User not found", "USER_NOT_FOUND");
        }

        return ToResponse(user);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var existingUser = await userRepository.GetByNormalizedEmailAsync(normalizedEmail);

        if (existingUser is not null)
        {
            throw new ConflictException("Email already exists", "EMAIL_ALREADY_EXISTS");
        }

        var user = new User
        {
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PasswordHash = "pending-auth",
            Role = "User",
            IsActive = true,
            IsEmailVerified = false
        };

        var createdUser = await userRepository.CreateAsync(user);

        return ToResponse(createdUser);
    }

    public async Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await userRepository.GetByIdAsync(id);

        if (user is null)
        {
            throw new NotFoundException("User not found", "USER_NOT_FOUND");
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var existingUser = await userRepository.GetByNormalizedEmailAsync(normalizedEmail);

        if (existingUser is not null && existingUser.Id != id)
        {
            throw new ConflictException("Email already exists", "EMAIL_ALREADY_EXISTS");
        }

        user.Email = request.Email.Trim();
        user.NormalizedEmail = normalizedEmail;

        await userRepository.UpdateAsync(user);

        return ToResponse(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);

        if (user is null)
        {
            throw new NotFoundException("User not found", "USER_NOT_FOUND");
        }

        await userRepository.DeleteAsync(user);
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse(
            user.Id,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
    }
}
