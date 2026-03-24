using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Infrastructure.Multitenancy;
using System.Security.Claims;

namespace RentalOS.Infrastructure.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public byte[]? TenantSlug { get; private set; }

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var tenantSlug = context.Request.Headers["X-Tenant-Slug"].ToString();

        if (!string.IsNullOrEmpty(tenantSlug))
        {
            var schemaName = $"tenant_{tenantSlug.Replace("-", "_")}";
            var userId = Guid.Empty;
            var role = RentalOS.Domain.Enums.UserRole.Staff;
            var plan = RentalOS.Domain.Enums.PlanType.Trial;

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null) Guid.TryParse(userIdClaim.Value, out userId);

                var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;
                if (!string.IsNullOrEmpty(roleClaim)) Enum.TryParse(roleClaim, true, out role);

                var planClaim = context.User.FindFirst("plan")?.Value;
                if (!string.IsNullOrEmpty(planClaim)) Enum.TryParse(planClaim, true, out plan);
            }

            tenantContext.Initialize(tenantSlug, schemaName, userId, role, plan);
        }

        await _next(context);
    }
}
