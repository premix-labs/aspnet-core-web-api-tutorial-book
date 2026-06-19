using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(320)]
    public required string Email { get; set; }

    [MaxLength(320)]
    public required string NormalizedEmail { get; set; }

    [MaxLength(512)]
    public required string PasswordHash { get; set; }

    [MaxLength(50)]
    public required string Role { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsEmailVerified { get; set; }

    public DateTimeOffset? EmailVerifiedAt { get; set; }

    public int AccessFailedCount { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public DateTimeOffset? PasswordChangedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = [];

    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
}
