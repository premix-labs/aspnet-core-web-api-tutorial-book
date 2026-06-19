using System.ComponentModel.DataAnnotations;
using Backend.Api.Constants;

namespace Backend.Api.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(320)]
    public string NormalizedEmail { get; set; } = string.Empty;

    [MaxLength(512)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Role { get; set; } = Roles.User;

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
