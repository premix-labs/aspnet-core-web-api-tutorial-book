using Microsoft.EntityFrameworkCore;
using Backend.Api.Constants;
using Backend.Api.Data;
using Backend.Api.Models;

namespace Backend.Api.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public IQueryable<User> Query()
    {
        return dbContext.Users;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return dbContext.Users.FirstOrDefaultAsync(
            user => user.NormalizedEmail == normalizedEmail,
            cancellationToken);
    }

    public Task<bool> NormalizedEmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return dbContext.Users.AnyAsync(
            user => user.NormalizedEmail == normalizedEmail,
            cancellationToken);
    }

    public Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken)
    {
        return dbContext.Users.CountAsync(
            user => user.Role == Roles.Admin && user.IsActive,
            cancellationToken);
    }

    public void Add(User user)
    {
        dbContext.Users.Add(user);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
