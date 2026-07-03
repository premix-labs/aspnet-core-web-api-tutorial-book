using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Backend.Api.Tests;

public class ProductionHardeningIntegrationTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    [Fact]
    public async Task HealthChecks_WhenCalled_ReturnSuccess()
    {
        var client = CreateClient();

        var liveResponse = await client.GetAsync("/health/live");
        var readyResponse = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, liveResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readyResponse.StatusCode);
    }

    [Fact]
    public async Task Response_IncludesSecurityHeaders()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal("nosniff", GetHeader(response, "X-Content-Type-Options"));
        Assert.Equal("DENY", GetHeader(response, "X-Frame-Options"));
        Assert.Equal("no-referrer", GetHeader(response, "Referrer-Policy"));
        Assert.Equal("same-origin", GetHeader(response, "Cross-Origin-Opener-Policy"));
        Assert.Equal("none", GetHeader(response, "X-Permitted-Cross-Domain-Policies"));
        Assert.Contains("no-store", GetHeader(response, "Cache-Control"));
        Assert.Contains("default-src 'none'", GetHeader(response, "Content-Security-Policy"));
        Assert.Contains("geolocation=()", GetHeader(response, "Permissions-Policy"));
    }

    [Fact]
    public async Task Response_IncludesCorrelationIdHeader()
    {
        var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Correlation-Id", "test-correlation-id");

        var response = await client.SendAsync(request);

        Assert.Equal("test-correlation-id", GetHeader(response, "X-Correlation-Id"));
    }

    [Fact]
    public async Task CorsPreflight_WhenOriginIsConfigured_ReturnsCorsHeaders()
    {
        var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", "http://localhost:3000");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("http://localhost:3000", GetHeader(response, "Access-Control-Allow-Origin"));
    }

    private HttpClient CreateClient()
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static string GetHeader(HttpResponseMessage response, string name)
    {
        Assert.True(
            response.Headers.TryGetValues(name, out var values),
            $"Missing response header '{name}'.");

        return string.Join(", ", values);
    }
}
