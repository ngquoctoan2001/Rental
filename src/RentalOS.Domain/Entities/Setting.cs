namespace RentalOS.Domain.Entities;

/// <summary>Key-value tenant configuration store — bảng 14.</summary>
public class Setting
{
    /// <summary>PK (UUID).</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Unique setting key (e.g. "payment.momo").</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Group name of the setting.</summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>JSON value for this setting.</summary>
    public string Value { get; set; } = "{}";

    /// <summary>FK to the last user who updated this setting.</summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>Timestamp of the last update.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
