namespace RentalOS.Application.Modules.Reports.Queries.GetDashboardSummary;

public class DashboardSummaryDto
{
    public RoomStatsDto Rooms { get; set; } = new();
    public RevenueStatsDto Revenue { get; set; } = new();
    public InvoiceStatsDto Invoices { get; set; } = new();
    public ContractStatsDto Contracts { get; set; } = new();
    public List<AlertDto> Alerts { get; set; } = new();
}

public class RoomStatsDto
{
    public int Total { get; set; }
    public int Available { get; set; }
    public int Rented { get; set; }
    public int Maintenance { get; set; }
    public double OccupancyRate { get; set; }
}

public class RevenueStatsDto
{
    public decimal ThisMonth { get; set; }
    public decimal LastMonth { get; set; }
    public double ChangePercent { get; set; }
}

public class InvoiceStatsDto
{
    public int PendingCount { get; set; }
    public int OverdueCount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal OverdueAmount { get; set; }
}

public class ContractStatsDto
{
    public int ExpiringIn30Days { get; set; }
    public int ExpiringIn7Days { get; set; }
}

public class AlertDto
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Message { get; set; } = string.Empty;
}
