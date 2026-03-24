namespace RentalOS.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    Guid? TenantId { get; }
    bool IsAuthenticated { get; }
}
