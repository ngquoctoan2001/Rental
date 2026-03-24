using RentalOS.Domain.Enums;

namespace RentalOS.Domain.Entities;

/// <summary>Public-schema tenant record — bảng 1 (public.tenants).</summary>
public class Tenant : BaseEntity
{
    /// <summary>URL-safe unique slug (e.g. "nha-tro-an-phat").</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Display name of the rental business.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Primary contact email of the owner.</summary>
    public string OwnerEmail { get; set; } = string.Empty;

    /// <summary>Full name of the owner.</summary>
    public string OwnerName { get; set; } = string.Empty;

    /// <summary>Owner phone number.</summary>
    public string? Phone { get; set; }

    /// <summary>Current subscription plan.</summary>
    public PlanType Plan { get; set; } = PlanType.Trial;

    /// <summary>When the current paid plan expires.</summary>
    public DateTime? PlanExpiresAt { get; set; }

    /// <summary>When the free trial ends.</summary>
    public DateTime? TrialEndsAt { get; set; }

    /// <summary>PostgreSQL schema name for this tenant (e.g. "tenant_an_phat").</summary>
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>Whether this tenant can log in and use the system.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Whether the onboarding wizard has been completed.</summary>
    public bool OnboardingDone { get; set; } = false;

    // Navigation
    public ICollection<SubscriptionPayment> SubscriptionPayments { get; set; } = [];
}
