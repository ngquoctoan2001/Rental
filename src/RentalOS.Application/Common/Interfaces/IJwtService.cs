using System.Security.Claims;

namespace RentalOS.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(string userId, string tenantSlug, string role, string email, string plan);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
