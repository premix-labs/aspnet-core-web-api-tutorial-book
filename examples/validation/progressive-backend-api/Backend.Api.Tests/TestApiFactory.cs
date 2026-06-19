using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Backend.Api.Data;
using Backend.Api.Services;

namespace Backend.Api.Tests;

public class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> previousValues = [];
    private readonly string databaseName = $"BackendApiValidationTests-{Guid.NewGuid()}";

    public TestEmailSender EmailSender { get; } = new();

    public TestApiFactory()
    {
        SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            "Server=localhost,1433;Database=BackendApiTests;User Id=sa;Password=Test_Local_Password_123!;TrustServerCertificate=True;");
        SetEnvironmentVariable("Jwt__Issuer", "Backend.Api");
        SetEnvironmentVariable("Jwt__Audience", "Backend.ApiClient");
        SetEnvironmentVariable(
            "Jwt__SigningKey",
            "test-signing-key-at-least-32-characters");
        SetEnvironmentVariable("Jwt__ExpirationMinutes", "60");
        SetEnvironmentVariable("DataSeeding__Enabled", "false");
        SetEnvironmentVariable("RefreshTokens__ExpirationDays", "30");
        SetEnvironmentVariable("AccountLockout__MaxFailedAccessAttempts", "5");
        SetEnvironmentVariable("AccountLockout__LockoutMinutes", "15");
        SetEnvironmentVariable("RateLimiting__AuthPermitLimit", "1000");
        SetEnvironmentVariable("Cors__AllowedOrigins__0", "http://localhost:3000");
        SetEnvironmentVariable(
            "EmailVerification__VerificationUrlTemplate",
            "http://localhost:5173/verify-email?token={token}");
        SetEnvironmentVariable(
            "PasswordReset__ResetUrlTemplate",
            "http://localhost:5173/reset-password?token={token}");
        SetEnvironmentVariable("Email__UseSmtp", "false");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<IEmailSender>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));

            services.AddSingleton<IEmailSender>(EmailSender);
        });
    }

    protected override void Dispose(bool disposing)
    {
        foreach (var (key, value) in previousValues)
        {
            Environment.SetEnvironmentVariable(key, value);
        }

        base.Dispose(disposing);
    }

    private void SetEnvironmentVariable(string key, string value)
    {
        previousValues[key] = Environment.GetEnvironmentVariable(key);
        Environment.SetEnvironmentVariable(key, value);
    }
}
