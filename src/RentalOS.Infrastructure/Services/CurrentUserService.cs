using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) 
                             ?? httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

    public string? Role => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role) 
                            ?? httpContextAccessor.HttpContext?.User?.FindFirstValue("role");

    public string? TenantSlug => httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_slug");

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}

