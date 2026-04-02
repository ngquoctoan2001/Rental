using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalOS.Infrastructure.Persistence;

namespace RentalOS.API.Controllers;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/v1/[controller]")]
public class AdminController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet("landlords")]
    public async Task<ActionResult> GetLandlords(CancellationToken cancellationToken)
    {
        var tenants = await dbContext.Tenants
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Slug,
                t.OwnerEmail,
                t.OwnerName,
                t.Phone,
                Plan = t.Plan.ToString(),
                t.IsActive,
                t.OnboardingDone,
                t.TrialEndsAt,
                t.PlanExpiresAt,
                t.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return Ok(tenants);
    }

    [HttpPatch("landlords/{id}/toggle-active")]
    public async Task<ActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (tenant == null) return NotFound();

        tenant.IsActive = !tenant.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { tenant.Id, tenant.IsActive });
    }
}
