namespace RentalOS.Application.Modules.Reports.Queries.GetCollectionRateReport;

public class CollectionRateDto
{
    public decimal TotalInvoiced { get; set; }
    public decimal TotalCollected { get; set; }
    public double Rate { get; set; }
    public List<CollectionRateByMonthDto> Details { get; set; } = new();
}

public class CollectionRateByMonthDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Invoiced { get; set; }
    public decimal Collected { get; set; }
    public double Rate { get; set; }
}
