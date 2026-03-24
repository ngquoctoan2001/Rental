using RentalOS.Domain.Enums;

namespace RentalOS.Domain.Entities;

/// <summary>A rentable room (phòng trọ) belonging to a property — bảng 5.</summary>
public class Room : BaseEntity
{
    /// <summary>FK to the parent property.</summary>
    public Guid PropertyId { get; set; }

    /// <summary>Human-readable room identifier (e.g. "P101").</summary>
    public string RoomNumber { get; set; } = string.Empty;

    /// <summary>Floor number.</summary>
    public int Floor { get; set; } = 1;

    /// <summary>Room area in square metres.</summary>
    public decimal? AreaSqm { get; set; }

    /// <summary>Monthly base rent price (VND).</summary>
    public decimal BasePrice { get; set; }

    /// <summary>Electricity price per kWh (VND). Default: 3 500.</summary>
    public decimal ElectricityPrice { get; set; } = 3500;

    /// <summary>Water price per m³ (VND). Default: 15 000.</summary>
    public decimal WaterPrice { get; set; } = 15000;

    /// <summary>Fixed monthly service fee (VND).</summary>
    public decimal ServiceFee { get; set; } = 0;

    /// <summary>Fixed monthly internet fee (VND). MỚI.</summary>
    public decimal InternetFee { get; set; } = 0;

    /// <summary>Fixed monthly garbage collection fee (VND). MỚI.</summary>
    public decimal GarbageFee { get; set; } = 0;

    /// <summary>Current operational status.</summary>
    public RoomStatus Status { get; set; } = RoomStatus.Available;

    /// <summary>JSON array of amenity labels.</summary>
    public string Amenities { get; set; } = "[]";

    public bool IsActive { get; set; } = true;

    /// <summary>JSON array of image URLs.</summary>
    public string Images { get; set; } = "[]";

    /// <summary>General notes about the room.</summary>
    public string? Notes { get; set; }

    /// <summary>Reason why the room is under maintenance. MỚI.</summary>
    public string? MaintenanceNote { get; set; }

    /// <summary>Date when maintenance started. MỚI.</summary>
    public DateOnly? MaintenanceSince { get; set; }

    // Navigation
    public Property Property { get; set; } = null!;
    public ICollection<Contract> Contracts { get; set; } = [];
    public ICollection<MeterReading> MeterReadings { get; set; } = [];
}
