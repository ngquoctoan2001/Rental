namespace RentalOS.Domain.Entities;

/// <summary>In-app bell notification shown to a specific user.</summary>
public class InAppNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The user this notification is shown to.</summary>
    public Guid UserId { get; set; }

    /// <summary>Notification category (e.g. "invoice_overdue", "contract_expiring").</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Short notification title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Full notification message body.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Whether the user has read/dismissed this notification.</summary>
    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
