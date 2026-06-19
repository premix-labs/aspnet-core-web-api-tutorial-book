using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Constants;
using Backend.Api.Models;

namespace Backend.Api.Data;

public static class DatabaseSeeder
{
    private const string DefaultPassword = "Passw0rd!";

    public static async Task SeedAsync(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.Users.AnyAsync(user => user.NormalizedEmail == "ADMIN@EXAMPLE.COM", cancellationToken))
        {
            dbContext.Users.Add(CreateUser("admin@example.com", Roles.Admin, passwordHasher));
        }

        if (!await dbContext.Users.AnyAsync(user => user.NormalizedEmail == "USER@EXAMPLE.COM", cancellationToken))
        {
            dbContext.Users.Add(CreateUser("user@example.com", Roles.User, passwordHasher));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static User CreateUser(
        string email,
        string role,
        IPasswordHasher<User> passwordHasher)
    {
        var user = new User
        {
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = string.Empty,
            Role = role,
            IsActive = true,
            IsEmailVerified = true,
            EmailVerifiedAt = DateTimeOffset.UtcNow,
            PasswordChangedAt = DateTimeOffset.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, DefaultPassword);

        return user;
    }
}
