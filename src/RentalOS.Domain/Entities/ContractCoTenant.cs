namespace RentalOS.Domain.Entities;

/// <summary>Co-tenants sharing a room under a contract — bảng 8 (MỚI).</summary>
public class ContractCoTenant : BaseEntity
{
    /// <summary>FK to the parent contract.</summary>
    public Guid ContractId { get; set; }

    /// <summary>FK to the customer who is co-residing.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>True when this record represents the primary contract signee.</summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>Date the co-tenant moved in.</summary>
    public DateOnly? MovedInAt { get; set; }

    /// <summary>Date the co-tenant moved out (null = still residing).</summary>
    public DateOnly? MovedOutAt { get; set; }

    // Navigation
    public Contract Contract { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
}
