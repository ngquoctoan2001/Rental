namespace RentalOS.Application.Modules.Reports.Queries.GetTopRooms;

public class TopRoomDto
{
    public string RoomNumber { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int OccupancyMonths { get; set; }
}
