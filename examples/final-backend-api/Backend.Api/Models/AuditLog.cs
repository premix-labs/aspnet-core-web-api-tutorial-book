using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Models;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? ActorUserId { get; set; }

    [MaxLength(100)]
    public required string Action { get; set; }

    [MaxLength(100)]
    public required string EntityName { get; set; }

    [MaxLength(100)]
    public required string EntityId { get; set; }

    [MaxLength(1000)]
    public string? Detail { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
