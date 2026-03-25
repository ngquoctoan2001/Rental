namespace RentalOS.Application.Modules.Reports.Queries.GetOverdueTrend;

public class OverdueTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int OverdueCount { get; set; }
    public decimal OverdueAmount { get; set; }
}

// Separate file for TopRooms if needed, or keeping here for simplicity
namespace RentalOS.Application.Modules.Reports.Queries.GetTopRooms;

public class TopRoomDto
{
    public string RoomNumber { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int OccupancyMonths { get; set; }
}
