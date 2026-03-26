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
        Console.WriteLine($"[TenantMiddleware] Request Path: {path}");


        // Skip public endpoints OR handle special public endpoints that need tenant context
        if (IsPublicEndpoint(path))
        {
            // Special case: public invoice endpoint needs to find the tenant by token
            if (path.StartsWith("/api/v1/public/invoice/"))
            {
                var token = path.Split('/').Last();
                if (!string.IsNullOrEmpty(token))
                {
                    var publicTenant = await dbContext.FindTenantByInvoiceTokenAsync(token);
                    if (publicTenant != null)
                    {
                        var publicSchemaName = publicTenant.SchemaName;
                        var publicSetSearchPathSql = $"SET search_path TO \"{publicSchemaName}\", public";
                        await dbContext.Database.ExecuteSqlRawAsync(publicSetSearchPathSql);
                        
                        // Initialize tenant context with minimal info (no user)
                        tenantContext.Initialize(publicTenant.Id, publicTenant.Slug, publicSchemaName, Guid.Empty, RentalOS.Domain.Enums.UserRole.Staff, RentalOS.Domain.Enums.PlanType.Trial);
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        await context.Response.WriteAsJsonAsync(new { error = "Payment link not found or expired" });
                        return;
                    }
                }
            }

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
        
        var role = Enum.TryParse<RentalOS.Domain.Enums.UserRole>(roleClaim, true, out var r) ? r : RentalOS.Domain.Enums.UserRole.Staff;
        
        var planClaim = context.User.FindFirst("plan")?.Value;
        var plan = Enum.TryParse<RentalOS.Domain.Enums.PlanType>(planClaim, true, out var p) ? p : RentalOS.Domain.Enums.PlanType.Trial;


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
        return path.StartsWith("/api/v1/auth/") || 
               path.StartsWith("/auth/") || 
               path.StartsWith("/pay/") || 
               path.StartsWith("/api/v1/public/") ||
               path.Contains("/payments/") && (path.EndsWith("/webhook") || path.EndsWith("/return")) ||
               path == "/health" || 
               path == "/health/liveness" ||
               path.StartsWith("/openapi") ||
               path.StartsWith("/scalar") ||
               path.StartsWith("/swagger") ||
               path.StartsWith("/hangfire") ||
               path.StartsWith("/hubs/");
    }
}

