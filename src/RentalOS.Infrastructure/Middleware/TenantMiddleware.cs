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

        // If X-Tenant-Slug is missing, try to resolve from invoice token in URL
        if (string.IsNullOrEmpty(tenantSlug) && context.Request.Path.Value != null && 
            context.Request.Path.Value.Contains("/api/invoice/detail/", StringComparison.OrdinalIgnoreCase))
        {
            var pathParts = context.Request.Path.Value.Split('/');
            var token = pathParts.Last();
            
            if (!string.IsNullOrEmpty(token))
            {
                var dbContext = context.RequestServices.GetRequiredService<IApplicationDbContext>();
                var tenant = await dbContext.FindTenantByInvoiceTokenAsync(token);
                if (tenant != null)
                {
                    tenantSlug = tenant.Slug;
                }
            }
        }

        if (!string.IsNullOrEmpty(tenantSlug))
        {
            var schemaName = $"tenant_{tenantSlug.Replace("-", "_")}";
            // Default identity values for public access
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

            tenantContext.Initialize(Guid.Empty, tenantSlug, schemaName, userId, role, plan);
        }

        await _next(context);
    }
}
