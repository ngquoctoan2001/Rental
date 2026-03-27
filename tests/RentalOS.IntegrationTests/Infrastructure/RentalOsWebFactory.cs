using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RentalOS.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that overrides configuration for integration tests.
/// Uses an in-memory or test-specific database connection string.
/// </summary>
public class RentalOsWebFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override with test-specific settings (can be overridden per test class)
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Tests should provide a real PostgreSQL or use a test DB
                // These values are intentionally empty — tests that need DB will skip if not configured
                ["ConnectionStrings:DefaultConnection"] = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION") ?? "",
                ["Jwt:Key"] = "test-secret-key-that-is-at-least-32-chars-long!!",
                ["Jwt:Issuer"] = "RentalOS.Test",
                ["Jwt:Audience"] = "RentalOS.Test",
                ["Hangfire:ConnectionString"] = "",
            });
        });
    }
}
