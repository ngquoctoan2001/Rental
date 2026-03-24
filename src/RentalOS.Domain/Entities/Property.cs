namespace RentalOS.Domain.Entities;

/// <summary>A rental property (nhà trọ) that contains one or more rooms — bảng 4.</summary>
public class Property : BaseEntity
{
    /// <summary>Friendly name of the property.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Full street address.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Type of the property (e.g. "Apartment").</summary>
    public string PropertyType { get; set; } = string.Empty;

    /// <summary>FK to the owner user.</summary>
    public Guid OwnerId { get; set; }

    /// <summary>Province / city.</summary>
    public string? Province { get; set; }

    /// <summary>District (quận/huyện).</summary>
    public string? District { get; set; }

    /// <summary>Ward (phường/xã).</summary>
    public string? Ward { get; set; }

    /// <summary>GPS latitude (up to 7 decimal places).</summary>
    public decimal? Lat { get; set; }

    /// <summary>GPS longitude (up to 7 decimal places).</summary>
    public decimal? Lng { get; set; }

    /// <summary>Optional free-text description.</summary>
    public string? Description { get; set; }

    /// <summary>URL of the primary cover image.</summary>
    public string? CoverImage { get; set; }

    /// <summary>JSON array of additional image URLs.</summary>
    public string Images { get; set; } = "[]";

    /// <summary>Total number of floors in the building.</summary>
    public int TotalFloors { get; set; } = 1;

    /// <summary>Whether the property is active and visible.</summary>
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Room> Rooms { get; set; } = [];
    public ICollection<Address> Addresses { get; set; } = [];
}
