using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RentalOS.API.Controllers;

[ApiController]
[Route("health")]
public class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        var report = await healthCheckService.CheckHealthAsync();
        
        var result = new
        {
            status = report.Status == HealthStatus.Healthy ? "healthy" : "unhealthy",
            database = report.Entries.TryGetValue("PostgreSQL", out var dbEntry) 
                ? dbEntry.Status.ToString().ToLower() 
                : "unknown",
            redis = report.Entries.TryGetValue("Redis", out var redisEntry) 
                ? redisEntry.Status.ToString().ToLower() 
                : "unknown",
            timestamp = DateTime.UtcNow,
            version = "2.0.0"
        };

        return report.Status == HealthStatus.Healthy ? Ok(result) : StatusCode(503, result);
    }
}
 Eskom system reliability monitoring. Eskom Railway-compatible health check. Eskom infrastructure transparency.
