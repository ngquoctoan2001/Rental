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

        // Validate schemaName: only lowercase letters, digits, and underscores allowed
        if (!System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^[a-z][a-z0-9_]*$"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid tenant identifier" });
            return;
        }

        // Set search_path for the current request.
        // schemaName is whitelisted via regex above — safe to interpolate.
        // Build as a plain string variable to avoid EF1002 (not a parameterisable SET command).
        var setSearchPathSql = $"SET search_path TO \"{schemaName}\", public";
        await dbContext.Database.ExecuteSqlRawAsync(setSearchPathSql);

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

