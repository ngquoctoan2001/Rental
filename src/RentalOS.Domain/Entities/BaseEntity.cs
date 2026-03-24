namespace RentalOS.Domain.Entities;

/// <summary>Base entity with common audit fields for all per-tenant entities.</summary>
public abstract class BaseEntity : IEntity
{
    /// <summary>Primary key (UUID v4).</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Timestamp when the record was created (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Timestamp when the record was last updated (UTC).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
