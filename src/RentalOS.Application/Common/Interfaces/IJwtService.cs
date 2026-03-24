using RentalOS.Domain.Entities;

namespace RentalOS.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user);
    string GenerateRefreshToken();
}
