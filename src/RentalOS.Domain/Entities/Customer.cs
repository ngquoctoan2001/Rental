namespace RentalOS.Domain.Entities;

/// <summary>A tenant / customer who rents or has rented a room — bảng 6.</summary>
public class Customer : BaseEntity
{
    /// <summary>Full legal name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Primary contact phone.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Optional contact email.</summary>
    public string? Email { get; set; }

    /// <summary>National ID / CCCD number.</summary>
    public string? IdCardNumber { get; set; }

    /// <summary>URL of the front-side ID card image.</summary>
    public string? IdCardImageFront { get; set; }

    /// <summary>URL of the back-side ID card image.</summary>
    public string? IdCardImageBack { get; set; }

    /// <summary>URL of the customer's portrait photo.</summary>
    public string? PortraitImage { get; set; }

    /// <summary>Date of birth.</summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>Gender (male / female / other).</summary>
    public string? Gender { get; set; }

    /// <summary>Home province / hometown.</summary>
    public string? Hometown { get; set; }

    /// <summary>Current residing address. MỚI.</summary>
    public string? CurrentAddress { get; set; }

    /// <summary>Occupation / profession. MỚI.</summary>
    public string? Occupation { get; set; }

    /// <summary>Name of workplace or employer. MỚI.</summary>
    public string? Workplace { get; set; }

    /// <summary>Emergency contact person's full name.</summary>
    public string? EmergencyContactName { get; set; }

    /// <summary>Emergency contact phone number.</summary>
    public string? EmergencyContactPhone { get; set; }

    /// <summary>Relationship of emergency contact to customer. MỚI.</summary>
    public string? EmergencyContactRelationship { get; set; }

    /// <summary>Whether this customer is blacklisted.</summary>
    public bool IsBlacklisted { get; set; } = false;

    /// <summary>Reason for blacklisting if applicable.</summary>
    public string? BlacklistReason { get; set; }

    /// <summary>Timestamp when the customer was blacklisted.</summary>
    public DateTime? BlacklistedAt { get; set; }

    /// <summary>FK to the user who blacklisted this customer.</summary>
    public Guid? BlacklistedBy { get; set; }

    /// <summary>General internal notes.</summary>
    public string? Notes { get; set; }

    // Navigation
    public ICollection<Contract> Contracts { get; set; } = [];
    public ICollection<ContractCoTenant> CoTenantEntries { get; set; } = [];
}
