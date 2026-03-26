using RentalOS.Domain.Enums;

namespace RentalOS.Application.Common.Interfaces;

public interface ITenantContext
{
    Guid TenantId { get; }
    string? TenantSlug { get; }
    string? SchemaName { get; }
    Guid UserId { get; }
    UserRole UserRole { get; }
    PlanType PlanType { get; }
    bool IsInitialized { get; }

    void Initialize(Guid tenantId, string tenantSlug, string schemaName, Guid userId, UserRole role, PlanType plan);
}
