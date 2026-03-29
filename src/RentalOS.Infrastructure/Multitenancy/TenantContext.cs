using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Enums;

namespace RentalOS.Infrastructure.Multitenancy;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }
    public string? TenantSlug { get; private set; }
    public string? SchemaName { get; private set; }
    public Guid UserId { get; private set; }
    public RentalOS.Domain.Enums.UserRole Role { get; private set; }
    public RentalOS.Domain.Enums.PlanType Plan { get; private set; }
    public DateTime? TrialEndsAt { get; private set; }
    public DateTime? PlanExpiresAt { get; private set; }
    public bool IsInitialized => !string.IsNullOrEmpty(TenantSlug);

    public void Initialize(Guid tenantId, string tenantSlug, string schemaName, Guid userId, RentalOS.Domain.Enums.UserRole role, RentalOS.Domain.Enums.PlanType plan, DateTime? trialEndsAt, DateTime? planExpiresAt)
    {
        TenantId = tenantId;
        TenantSlug = tenantSlug;
        SchemaName = schemaName;
        UserId = userId;
        Role = role;
        Plan = plan;
        TrialEndsAt = trialEndsAt;
        PlanExpiresAt = planExpiresAt;
    }
}
