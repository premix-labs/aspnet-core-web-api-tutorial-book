using Microsoft.EntityFrameworkCore;
using Backend.Api.Constants;
using Backend.Api.Data;
using Backend.Api.Dtos.Admin;
using Backend.Api.Dtos.Common;
using Backend.Api.Models;

namespace Backend.Api.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<List<User>> GetAllAsync()
    {
        return db.Users
            .AsNoTracking()
            .OrderBy(user => user.Id)
            .ToListAsync();
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        return db.Users.FirstOrDefaultAsync(user => user.Id == id);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        return db.Users.FirstOrDefaultAsync(user => user.Email == email);
    }

    public Task<User?> GetByNormalizedEmailAsync(string normalizedEmail)
    {
        return db.Users.FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail);
    }

    public Task<bool> NormalizedEmailExistsAsync(string normalizedEmail)
    {
        return db.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail);
    }

    public async Task<User> CreateAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(User user)
    {
        db.Users.Remove(user);
        await db.SaveChangesAsync();

        return true;
    }

    public Task<int> CountActiveAdminsAsync()
    {
        return db.Users.CountAsync(user =>
            user.Role == Roles.Admin &&
            user.IsActive);
    }

    public async Task<PagedResponse<User>> QueryUsersAsync(AdminUserQuery query)
    {
        var users = db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToUpperInvariant();

            users = users.Where(user =>
                user.NormalizedEmail.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var role = Roles.IsValid(query.Role)
                ? Roles.Normalize(query.Role)
                : query.Role;

            users = users.Where(user => user.Role == role);
        }

        if (query.IsActive.HasValue)
        {
            users = users.Where(user => user.IsActive == query.IsActive.Value);
        }

        users = (query.SortBy.ToLowerInvariant(), query.SortDirection.ToLowerInvariant()) switch
        {
            ("email", "asc") => users.OrderBy(user => user.Email),
            ("email", "desc") => users.OrderByDescending(user => user.Email),
            ("role", "asc") => users.OrderBy(user => user.Role),
            ("role", "desc") => users.OrderByDescending(user => user.Role),
            ("createdat", "asc") => users.OrderBy(user => user.CreatedAt),
            _ => users.OrderByDescending(user => user.CreatedAt)
        };

        var totalItems = await users.CountAsync();

        var items = await users
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResponse<User>(items, query.Page, query.PageSize, totalItems);
    }
}
