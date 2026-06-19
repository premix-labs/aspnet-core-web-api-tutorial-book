using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Backend.Api.Data;
using Backend.Api.Services;

namespace Backend.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> _previousValues = [];
    private readonly string _databaseName = $"BackendApiTests-{Guid.NewGuid()}";

    public TestEmailSender EmailSender { get; } = new();

    public CustomWebApplicationFactory()
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
        SetEnvironmentVariable("Database__ApplyMigrationsOnStartup", "false");
        SetEnvironmentVariable("Database__SeedOnStartup", "false");
        SetEnvironmentVariable("RateLimiting__AuthPermitLimit", "1000");
        SetEnvironmentVariable("Cors__AllowedOrigins__0", "http://localhost:3000");
        SetEnvironmentVariable("Email__UseSmtp", "false");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("RateLimiting:AuthPermitLimit", "1000");
        builder.UseSetting("Cors:AllowedOrigins:0", "http://localhost:3000");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<IEmailSender>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.AddSingleton<IEmailSender>(EmailSender);
        });
    }

    protected override void Dispose(bool disposing)
    {
        foreach (var (key, value) in _previousValues)
        {
            Environment.SetEnvironmentVariable(key, value);
        }

        base.Dispose(disposing);
    }

    private void SetEnvironmentVariable(string key, string value)
    {
        _previousValues[key] = Environment.GetEnvironmentVariable(key);
        Environment.SetEnvironmentVariable(key, value);
    }
}
