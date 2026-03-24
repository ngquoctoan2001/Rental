namespace RentalOS.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? TenantSlug { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}

