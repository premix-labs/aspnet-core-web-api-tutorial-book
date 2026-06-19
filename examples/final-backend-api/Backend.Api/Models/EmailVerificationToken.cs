using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Models;

public class EmailVerificationToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [MaxLength(128)]
    public required string TokenHash { get; set; }

    [MaxLength(45)]
    public string? CreatedByIp { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public User User { get; set; } = null!;

    public bool IsActive(DateTimeOffset utcNow)
    {
        return UsedAt is null && RevokedAt is null && ExpiresAt > utcNow;
    }
}
