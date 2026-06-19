using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Constants;
using Backend.Api.Models;

namespace Backend.Api.Data;

public class DataSeeder(AppDbContext db, IPasswordHasher<User> passwordHasher)
{
    public async Task SeedAsync()
    {
        await EnsureUserAsync(
            "admin@example.com",
            "Admin1234!",
            Roles.Admin,
            isActive: true);

        await EnsureUserAsync(
            "demo-user@example.com",
            "User1234!",
            Roles.User,
            isActive: true);

        await EnsureUserAsync(
            "inactive-user@example.com",
            "User1234!",
            Roles.User,
            isActive: false);

        await db.SaveChangesAsync();
    }

    private async Task EnsureUserAsync(
        string email,
        string password,
        string role,
        bool isActive)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await db.Users.FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail);

        if (user is null)
        {
            user = new User
            {
                Email = email,
                NormalizedEmail = normalizedEmail,
                Role = role,
                IsActive = isActive,
                IsEmailVerified = true,
                EmailVerifiedAt = DateTimeOffset.UtcNow,
                PasswordChangedAt = DateTimeOffset.UtcNow
            };

            user.PasswordHash = passwordHasher.HashPassword(user, password);
            db.Users.Add(user);

            return;
        }

        if (user.PasswordHash == "pending-auth")
        {
            user.PasswordHash = passwordHasher.HashPassword(user, password);
        }

        user.Role = role;
        user.IsActive = isActive;
        user.NormalizedEmail = normalizedEmail;
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
    }
}
