using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Infrastructure.Multitenancy;

namespace RentalOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController(ITenantContext tenantContext) : ControllerBase
{
    private readonly ITenantContext _tenantContext = tenantContext;

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Online",
            timestamp = DateTime.UtcNow,
            tenant = _tenantContext.IsInitialized ? _tenantContext.TenantSlug : "Public",
            schema = _tenantContext.IsInitialized ? _tenantContext.SchemaName : "public"
        });
    }
}
