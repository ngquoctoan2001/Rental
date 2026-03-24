using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Enums;

namespace RentalOS.Infrastructure.Multitenancy;

public class TenantContext : ITenantContext
{
    public string? TenantSlug { get; private set; }
    public string? SchemaName { get; private set; }
    public Guid UserId { get; private set; }
    public UserRole UserRole { get; private set; }
    public PlanType PlanType { get; private set; }
    public bool IsInitialized => !string.IsNullOrEmpty(TenantSlug);

    public void Initialize(string tenantSlug, string schemaName, Guid userId, UserRole role, PlanType plan)
    {
        TenantSlug = tenantSlug;
        SchemaName = schemaName;
        UserId = userId;
        UserRole = role;
        PlanType = plan;
    }
}
