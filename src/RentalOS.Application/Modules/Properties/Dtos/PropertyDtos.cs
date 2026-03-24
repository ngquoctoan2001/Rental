namespace RentalOS.Application.Modules.Properties.Dtos;

public record PropertyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string? Province { get; init; }
    public string? District { get; init; }
    public string? Ward { get; init; }
    public string? Description { get; init; }
    public string? CoverImage { get; init; }
    public List<string> Images { get; init; } = [];
    public int TotalFloors { get; init; }
    public bool IsActive { get; init; }
    public RoomSummaryDto RoomSummary { get; init; } = new();
}

public record RoomSummaryDto
{
    public int Total { get; init; }
    public int Available { get; init; }
    public int Rented { get; init; }
    public int Maintenance { get; init; }
}

public record PropertyListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string? CoverImage { get; init; }
    public bool IsActive { get; init; }
    public RoomSummaryDto RoomSummary { get; init; } = new();
}

public record PropertyStatsDto
{
    public int TotalRooms { get; init; }
    public int AvailableRooms { get; init; }
    public int RentedRooms { get; init; }
    public int MaintenanceRooms { get; init; }
    public double OccupancyRate { get; init; }
    public decimal MonthlyRevenue { get; init; }
    public int OutstandingInvoices { get; init; }
    public decimal TotalOutstandingAmount { get; init; }
    public int UpcomingContractExpiries { get; init; }
    public decimal AverageRent { get; init; }
}
