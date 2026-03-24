namespace RentalOS.Domain.Entities;

/// <summary>Tracks every interaction with a public payment link — bảng 16 (MỚI).</summary>
public class PaymentLinkLog
{
    /// <summary>PK (UUID).</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FK to the invoice this link belongs to.</summary>
    public Guid InvoiceId { get; set; }

    /// <summary>IP address of the visitor.</summary>
    public string? IpAddress { get; set; }

    /// <summary>Browser/client user-agent string.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Action performed (e.g. "viewed", "momo_initiated", "vnpay_initiated", "bank_transfer_viewed").</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Timestamp of the action (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Invoice Invoice { get; set; } = null!;
}
