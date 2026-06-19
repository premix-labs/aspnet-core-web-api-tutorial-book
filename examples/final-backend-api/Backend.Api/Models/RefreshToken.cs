using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Models;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid FamilyId { get; set; } = Guid.NewGuid();

    [MaxLength(128)]
    public required string TokenHash { get; set; }

    [MaxLength(45)]
    public string? CreatedByIp { get; set; }

    [MaxLength(512)]
    public string? UserAgent { get; set; }

    [MaxLength(45)]
    public string? RevokedByIp { get; set; }

    [MaxLength(128)]
    public string? ReplacedByTokenHash { get; set; }

    [MaxLength(100)]
    public string? RevocationReason { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public User User { get; set; } = null!;

    public bool IsActive(DateTimeOffset utcNow)
    {
        return RevokedAt is null && ExpiresAt > utcNow;
    }
}
