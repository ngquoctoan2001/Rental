using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Enums;
using RentalOS.Infrastructure.Persistence;

namespace RentalOS.API.Middleware;

public class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, RentalOS.Application.Common.Interfaces.ITenantContext tenantContext)
    {
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        // Skip public endpoints
        if (IsPublicEndpoint(path))
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var tenantSlug = context.User.FindFirst("tenant_slug")?.Value;
        if (string.IsNullOrEmpty(tenantSlug))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant claim missing" });
            return;
        }

        // Verify tenant exists and is active in public schema
        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == tenantSlug);

        if (tenant == null || !tenant.IsActive)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant is inactive or does not exist" });
            return;
        }

        // Extract detailed claims
        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                          ?? context.User.FindFirst("sub")?.Value;
        
        Guid.TryParse(userIdClaim, out var userId);
        
        var roleClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value 
                        ?? context.User.FindFirst("role")?.Value;
        
        var role = Enum.TryParse<UserRole>(roleClaim, true, out var r) ? r : UserRole.Staff;
        
        var planClaim = context.User.FindFirst("plan")?.Value;
        var plan = Enum.TryParse<PlanType>(planClaim, true, out var p) ? p : PlanType.Trial;


        var schemaName = $"tenant_{tenantSlug.Replace("-", "_")}";

        // Set search_path for the current request
        await dbContext.Database.ExecuteSqlRawAsync($"SET search_path TO \"{schemaName}\", public");

        // Initialize Scoped TenantContext
        tenantContext.Initialize(tenant.Id, tenantSlug, schemaName, userId, role, plan);

        await next(context);
    }

    private static bool IsPublicEndpoint(string path)
    {
        return path.StartsWith("/auth/") || 
               path.StartsWith("/pay/") || 
               path.Contains("/payments/") && (path.EndsWith("/webhook") || path.EndsWith("/return")) ||
               path == "/health" || 
               path.StartsWith("/swagger");
    }
}

