namespace RentalOS.Domain.Entities;

/// <summary>A recorded electricity/water meter reading for a room — bảng 11.</summary>
public class MeterReading : BaseEntity
{
    /// <summary>FK to the room this reading belongs to.</summary>
    public Guid RoomId { get; set; }

    /// <summary>Date of the reading.</summary>
    public DateOnly ReadingDate { get; set; }

    /// <summary>Electricity meter value (kWh).</summary>
    public int ElectricityReading { get; set; }

    /// <summary>Water meter value (m³).</summary>
    public int WaterReading { get; set; }

    /// <summary>Photo of the electricity meter.</summary>
    public string? ElectricityImage { get; set; }

    /// <summary>Photo of the water meter.</summary>
    public string? WaterImage { get; set; }

    /// <summary>Optional note about the reading. MỚI.</summary>
    public string? Note { get; set; }

    /// <summary>FK to the user who recorded this reading.</summary>
    public Guid? RecordedBy { get; set; }

    // Navigation
    public Room Room { get; set; } = null!;
}
