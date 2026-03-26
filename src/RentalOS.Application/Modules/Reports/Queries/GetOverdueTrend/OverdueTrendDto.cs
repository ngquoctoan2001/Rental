namespace RentalOS.Application.Modules.Reports.Queries.GetOverdueTrend;

public class OverdueTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int OverdueCount { get; set; }
    public decimal OverdueAmount { get; set; }
}
