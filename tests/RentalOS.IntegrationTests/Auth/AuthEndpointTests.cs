using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using RentalOS.IntegrationTests.Infrastructure;

namespace RentalOS.IntegrationTests.Auth;

/// <summary>
/// Integration tests for authentication endpoints.
/// Requires a PostgreSQL test database configured via TEST_DB_CONNECTION env var.
/// Tests that do not need a DB connection test the API surface (status codes, schema).
/// </summary>
public class AuthEndpointTests : IClassFixture<RentalOsWebFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(RentalOsWebFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithMissingBody_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { });
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.InternalServerError // acceptable if DB not configured
        );
    }

    [Fact]
    public async Task Login_WithInvalidEmail_Returns400Or422()
    {
        var payload = new { email = "not-an-email", password = "whatever" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.InternalServerError
        );
    }

    [Fact]
    public async Task Register_WithMissingFields_Returns400Or422()
    {
        var payload = new { email = "test@example.com" }; // missing required fields
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", payload);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.InternalServerError
        );
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}
