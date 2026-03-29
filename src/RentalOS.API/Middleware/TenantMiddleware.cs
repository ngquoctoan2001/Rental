using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Enums;
using RentalOS.Infrastructure.Persistence;

using Microsoft.Extensions.Caching.Memory;

namespace RentalOS.API.Middleware;

public class TenantMiddleware(RequestDelegate next, IMemoryCache memoryCache)
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
                        
                        // Initialize tenant context with minimal info (no user)
                        tenantContext.Initialize(publicTenant.Id, publicTenant.Slug, publicSchemaName, Guid.Empty, RentalOS.Domain.Enums.UserRole.Tenant, RentalOS.Domain.Enums.PlanType.Trial, publicTenant.TrialEndsAt, publicTenant.PlanExpiresAt);

                        var publicSetSearchPathSql = $"SET search_path TO \"{publicSchemaName}\", public";
                        await dbContext.Database.ExecuteSqlRawAsync(publicSetSearchPathSql);
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

        // Verify tenant exists and is active in public schema using cache to reduce DB load
        var cacheKey = $"tenant_{tenantSlug}";
        var tenant = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Slug == tenantSlug);
        });

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
        
        var role = Enum.TryParse<RentalOS.Domain.Enums.UserRole>(roleClaim, true, out var r) ? r : RentalOS.Domain.Enums.UserRole.Tenant;
        
        var planClaim = context.User.FindFirst("plan")?.Value;
        var plan = Enum.TryParse<RentalOS.Domain.Enums.PlanType>(planClaim, true, out var p) ? p : RentalOS.Domain.Enums.PlanType.Trial;


        var schemaName = tenant.SchemaName;

        // Initialize Scoped TenantContext FIRST so interceptors and other services have access to tenant info immediately
        tenantContext.Initialize(tenant.Id, tenantSlug, schemaName, userId, role, plan, tenant.TrialEndsAt, tenant.PlanExpiresAt);

        // Set search_path for the current request. This ensures that the current DbContext
        // uses the correct schema even if the connection was already open.
        var setSearchPathSql = $"SET search_path TO \"{schemaName}\", public";
        await dbContext.Database.ExecuteSqlRawAsync(setSearchPathSql);

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
