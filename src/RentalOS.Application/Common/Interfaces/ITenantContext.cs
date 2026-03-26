using RentalOS.Domain.Enums;

namespace RentalOS.Application.Common.Interfaces;

public interface ITenantContext
{
    Guid TenantId { get; }
    string? TenantSlug { get; }
    string? SchemaName { get; }
    Guid UserId { get; }
    RentalOS.Domain.Enums.UserRole Role { get; }
    RentalOS.Domain.Enums.PlanType Plan { get; }
    bool IsInitialized { get; }

    void Initialize(Guid tenantId, string tenantSlug, string schemaName, Guid userId, RentalOS.Domain.Enums.UserRole role, RentalOS.Domain.Enums.PlanType plan);
}
