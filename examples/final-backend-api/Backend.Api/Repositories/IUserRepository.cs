using Backend.Api.Models;

namespace Backend.Api.Repositories;

public interface IUserRepository
{
    IQueryable<User> Query();

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<bool> NormalizedEmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken);

    void Add(User user);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
