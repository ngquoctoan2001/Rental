using RentalOS.Domain.Enums;

namespace RentalOS.Infrastructure.Multitenancy;

/// <summary>Provides per-request tenant context resolved from JWT claims.</summary>
public interface ITenantContext
{
    string TenantSlug { get; }
    string SchemaName { get; }
    Guid UserId { get; }
    UserRole UserRole { get; }
    PlanType PlanType { get; }
    bool IsInitialized { get; }
    void Initialize(string tenantSlug, string schemaName, Guid userId, UserRole role, PlanType plan);
}

/// <summary>Scoped implementation — populated by TenantMiddleware after JWT validation.</summary>
public sealed class TenantContext : ITenantContext
{
    public string TenantSlug { get; private set; } = string.Empty;
    public string SchemaName { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public UserRole UserRole { get; private set; }
    public PlanType PlanType { get; private set; }
    public bool IsInitialized { get; private set; }

    public void Initialize(string tenantSlug, string schemaName, Guid userId, UserRole role, PlanType plan)
    {
        TenantSlug = tenantSlug;
        SchemaName = schemaName;
        UserId = userId;
        UserRole = role;
        PlanType = plan;
        IsInitialized = true;
    }
}
