using Microsoft.AspNetCore.Http;
using RentalOS.Domain.Enums;
using RentalOS.Infrastructure.Multitenancy;
using System.Security.Claims;

namespace RentalOS.Infrastructure.Middleware;

/// <summary>
/// Extracts tenant and user information from JWT claims and populates ITenantContext.
/// This context is then used by EF Core interceptors for row-level/schema isolation.
/// </summary>
public class TenantMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var tenantSlug = user.FindFirstValue("tenant_slug");
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleStr = user.FindFirstValue(ClaimTypes.Role);
            var planStr = user.FindFirstValue("plan");

            if (!string.IsNullOrEmpty(tenantSlug) && Guid.TryParse(userIdStr, out var userId))
            {
                var schemaName = $"tenant_{tenantSlug.Replace("-", "_")}";
                Enum.TryParse<UserRole>(roleStr, true, out var role);
                Enum.TryParse<PlanType>(planStr, true, out var plan);

                tenantContext.Initialize(tenantSlug, schemaName, userId, role, plan);
            }
        }

        await _next(context);
    }
}
