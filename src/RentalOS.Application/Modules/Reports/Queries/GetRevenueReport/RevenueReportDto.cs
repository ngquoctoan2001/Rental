namespace RentalOS.Application.Modules.Reports.Queries.GetRevenueReport;

public class RevenueReportDto
{
    public PeriodDto Period { get; set; } = new();
    public RevenueSummaryDto Summary { get; set; } = new();
    public List<RevenueByMonthDto> ByMonth { get; set; } = new();
    public List<RevenueByPropertyDto> ByProperty { get; set; } = new();
    public Dictionary<string, decimal> ByMethod { get; set; } = new();
}

public class PeriodDto
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}

public class RevenueSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public double CollectionRate { get; set; }
    public decimal AvgMonthlyRevenue { get; set; }
}

public class RevenueByMonthDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Invoiced { get; set; }
    public decimal Collected { get; set; }
    public double Rate { get; set; }
}

public class RevenueByPropertyDto
{
    public string PropertyName { get; set; } = string.Empty;
    public decimal Invoiced { get; set; }
    public decimal Collected { get; set; }
    public double Rate { get; set; }
}
