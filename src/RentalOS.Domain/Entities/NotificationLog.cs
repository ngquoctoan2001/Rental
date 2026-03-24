using RentalOS.Domain.Enums;

namespace RentalOS.Domain.Entities;

/// <summary>Log of every notification sent (or attempted) across all channels — bảng 12 (MỚI).</summary>
public class NotificationLog : BaseEntity
{
    /// <summary>Event that triggered this notification (e.g. "invoice_created").</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Delivery channel used.</summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>Recipient's phone number (for Zalo/SMS).</summary>
    public string? RecipientPhone { get; set; }

    /// <summary>Recipient's email address.</summary>
    public string? RecipientEmail { get; set; }

    /// <summary>Recipient's display name.</summary>
    public string? RecipientName { get; set; }

    /// <summary>Message subject (used for email).</summary>
    public string? Subject { get; set; }

    /// <summary>Full message body / content.</summary>
    public string? Content { get; set; }

    /// <summary>Current delivery status.</summary>
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    /// <summary>Provider-side message ID returned after a successful send.</summary>
    public string? ProviderRef { get; set; }

    /// <summary>Error detail when delivery fails.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Number of send attempts made so far.</summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>Timestamp of the last successful delivery.</summary>
    public DateTime? SentAt { get; set; }

    /// <summary>Domain entity type related to this notification (e.g. "invoice").</summary>
    public string? EntityType { get; set; }

    /// <summary>PK of the related domain entity.</summary>
    public Guid? EntityId { get; set; }
}
