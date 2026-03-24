namespace RentalOS.Domain.Entities;

/// <summary>Immutable audit trail for all user actions — bảng 15.</summary>
public class AuditLog
{
    /// <summary>PK (UUID).</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FK to the user who performed the action (null for system actions).</summary>
    public Guid? UserId { get; set; }

    /// <summary>Snapshot of the user's display name at the time of the action.</summary>
    public string? UserName { get; set; }

    /// <summary>Action identifier (e.g. "invoices.send", "rooms.update").</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Domain entity type affected (e.g. "Invoice").</summary>
    public string? EntityType { get; set; }

    /// <summary>PK of the affected entity.</summary>
    public Guid? EntityId { get; set; }

    /// <summary>Human-readable code of the affected entity (e.g. "HD-2024-001"). MỚI.</summary>
    public string? EntityCode { get; set; }

    /// <summary>JSON snapshot of the entity before the change.</summary>
    public string? OldValue { get; set; }

    /// <summary>JSON snapshot of the entity after the change.</summary>
    public string? NewValue { get; set; }

    /// <summary>Client IP address.</summary>
    public string? IpAddress { get; set; }

    /// <summary>Client User-Agent string.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Timestamp of the action (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
