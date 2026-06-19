using Microsoft.EntityFrameworkCore;
using Backend.Api.Data;
using Backend.Api.Models;

namespace Backend.Api.Tests;

public class DatabaseModelTests
{
    [Fact]
    public void AppDbContext_HasProductionQueryIndexes()
    {
        using var dbContext = CreateDbContext();

        AssertIndex<User>(dbContext, new[] { nameof(User.NormalizedEmail) }, isUnique: true);
        AssertIndex<User>(dbContext, new[] { nameof(User.CreatedAt) });
        AssertIndex<User>(dbContext, new[] { nameof(User.Role), nameof(User.IsActive), nameof(User.CreatedAt) });

        AssertIndex<AuditLog>(dbContext, new[] { nameof(AuditLog.CreatedAt) });
        AssertIndex<AuditLog>(dbContext, new[] { nameof(AuditLog.ActorUserId), nameof(AuditLog.CreatedAt) });
        AssertIndex<AuditLog>(dbContext, new[] { nameof(AuditLog.EntityName), nameof(AuditLog.EntityId), nameof(AuditLog.CreatedAt) });

        AssertIndex<RefreshToken>(dbContext, new[] { nameof(RefreshToken.TokenHash) }, isUnique: true);
        AssertIndex<RefreshToken>(dbContext, new[] { nameof(RefreshToken.UserId), nameof(RefreshToken.ExpiresAt) });
        AssertIndex<RefreshToken>(dbContext, new[] { nameof(RefreshToken.UserId), nameof(RefreshToken.FamilyId) });

        AssertIndex<EmailVerificationToken>(dbContext, new[] { nameof(EmailVerificationToken.TokenHash) }, isUnique: true);
        AssertIndex<PasswordResetToken>(dbContext, new[] { nameof(PasswordResetToken.TokenHash) }, isUnique: true);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"DatabaseModelTests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static void AssertIndex<TEntity>(
        AppDbContext dbContext,
        string[] propertyNames,
        bool isUnique = false)
    {
        var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
        Assert.NotNull(entityType);

        var index = entityType.GetIndexes()
            .FirstOrDefault(index =>
                index.Properties.Select(property => property.Name).SequenceEqual(propertyNames));

        Assert.NotNull(index);
        Assert.Equal(isUnique, index.IsUnique);
    }
}
