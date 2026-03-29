using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Rooms.Dtos;

public class RoomDto
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal? AreaSqm { get; set; }
    public decimal BasePrice { get; set; }
    public decimal ElectricityPrice { get; set; }
    public decimal WaterPrice { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal InternetFee { get; set; }
    public decimal GarbageFee { get; set; }
    public RoomStatus Status { get; set; }
    public List<string> Amenities { get; set; } = [];
    public List<string> Images { get; set; } = [];
    public string? Notes { get; set; }
    public string? MaintenanceNote { get; set; }
    public DateOnly? MaintenanceSince { get; set; }
    public Guid? CurrentCustomerId { get; set; }
    public string? CurrentCustomerName { get; set; }
    public string? CurrentContractCode { get; set; }
}

public class RoomListItemDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal BasePrice { get; set; }
    public RoomStatus Status { get; set; }
    public string? PropertyName { get; set; }
    public Guid? CurrentCustomerId { get; set; }
    public string? CurrentCustomerName { get; set; }
    public string? CurrentContractCode { get; set; }
}

public class RoomHistoryDto
{
    public List<ContractSummaryDto> Contracts { get; set; } = [];
    public List<MaintenanceLogDto> MaintenanceLogs { get; set; } = [];
}

public class ContractSummaryDto
{
    public Guid Id { get; set; }
    public string ContractCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public ContractStatus Status { get; set; }
}

public class MaintenanceLogDto
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
