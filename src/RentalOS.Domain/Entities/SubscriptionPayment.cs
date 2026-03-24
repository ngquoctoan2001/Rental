using RentalOS.Domain.Enums;

namespace RentalOS.Domain.Entities;

/// <summary>Record of a subscription payment made by a tenant — bảng 2 (public.subscription_payments).</summary>
public class SubscriptionPayment
{
    /// <summary>PK (UUID).</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FK to the tenant who paid.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Plan purchased in this payment.</summary>
    public PlanType Plan { get; set; }

    /// <summary>Amount charged (VND).</summary>
    public decimal Amount { get; set; }

    /// <summary>Payment method used.</summary>
    public PaymentMethod Method { get; set; }

    /// <summary>Provider transaction reference (e.g. Momo order ID).</summary>
    public string? ProviderRef { get; set; }

    /// <summary>Payment status: pending | success | failed.</summary>
    public string Status { get; set; } = "pending";

    /// <summary>Start of billing period.</summary>
    public DateOnly BillingFrom { get; set; }

    /// <summary>End of billing period.</summary>
    public DateOnly BillingTo { get; set; }

    /// <summary>Timestamp when payment was confirmed.</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>Record creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}
