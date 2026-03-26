using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Enums;

namespace RentalOS.Infrastructure.Multitenancy;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }
    public string? TenantSlug { get; private set; }
    public string? SchemaName { get; private set; }
    public Guid UserId { get; private set; }
    public UserRole UserRole { get; private set; }
    public PlanType PlanType { get; private set; }
    public bool IsInitialized => !string.IsNullOrEmpty(TenantSlug);

    public void Initialize(Guid tenantId, string tenantSlug, string schemaName, Guid userId, UserRole role, PlanType plan)
    {
        TenantId = tenantId;
        TenantSlug = tenantSlug;
        SchemaName = schemaName;
        UserId = userId;
        UserRole = role;
        PlanType = plan;
    }
}
