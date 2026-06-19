using Microsoft.EntityFrameworkCore;
using Backend.Api.Models;

namespace Backend.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.NormalizedEmail).IsUnique();
            entity.HasIndex(user => user.CreatedAt);
            entity.HasIndex(user => new { user.Role, user.IsActive, user.CreatedAt });

            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.Property(user => user.NormalizedEmail).HasMaxLength(320).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(user => user.Role).HasMaxLength(50).IsRequired();
            entity.Property(user => user.IsActive).HasDefaultValue(true);
            entity.Property(user => user.IsEmailVerified).HasDefaultValue(false);
            entity.Property(user => user.CreatedAt).IsRequired();
            entity.Property(user => user.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<EmailVerificationToken>(entity =>
        {
            entity.HasKey(token => token.Id);
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasIndex(token => new { token.UserId, token.ExpiresAt });

            entity.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(token => token.CreatedByIp).HasMaxLength(45);
            entity.Property(token => token.CreatedAt).IsRequired();
            entity.Property(token => token.ExpiresAt).IsRequired();
            entity.HasOne(token => token.User)
                .WithMany(user => user.EmailVerificationTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(token => token.Id);
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasIndex(token => new { token.UserId, token.ExpiresAt });

            entity.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(token => token.CreatedByIp).HasMaxLength(45);
            entity.Property(token => token.CreatedAt).IsRequired();
            entity.Property(token => token.ExpiresAt).IsRequired();
            entity.HasOne(token => token.User)
                .WithMany(user => user.PasswordResetTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(log => log.Id);
            entity.HasIndex(log => log.CreatedAt);
            entity.HasIndex(log => new { log.ActorUserId, log.CreatedAt });
            entity.HasIndex(log => new { log.EntityName, log.EntityId, log.CreatedAt });
            entity.Property(log => log.Action).HasMaxLength(100).IsRequired();
            entity.Property(log => log.EntityName).HasMaxLength(100).IsRequired();
            entity.Property(log => log.EntityId).HasMaxLength(100).IsRequired();
            entity.Property(log => log.Detail).HasMaxLength(1000);
            entity.Property(log => log.IpAddress).HasMaxLength(45);
            entity.Property(log => log.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(token => token.Id);
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasIndex(token => new { token.UserId, token.ExpiresAt });
            entity.HasIndex(token => new { token.UserId, token.FamilyId });

            entity.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(token => token.CreatedByIp).HasMaxLength(45);
            entity.Property(token => token.UserAgent).HasMaxLength(512);
            entity.Property(token => token.RevokedByIp).HasMaxLength(45);
            entity.Property(token => token.ReplacedByTokenHash).HasMaxLength(128);
            entity.Property(token => token.RevocationReason).HasMaxLength(100);
            entity.Property(token => token.CreatedAt).IsRequired();
            entity.Property(token => token.ExpiresAt).IsRequired();
            entity.HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
