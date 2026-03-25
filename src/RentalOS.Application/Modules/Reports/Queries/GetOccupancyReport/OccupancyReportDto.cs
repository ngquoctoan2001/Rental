namespace RentalOS.Application.Modules.Reports.Queries.GetOccupancyReport;

public class OccupancyReportDto
{
    public OccupancyDetailDto Current { get; set; } = new();
    public List<OccupancyHistoryDto> History { get; set; } = new();
    public List<OccupancyByPropertyDto> ByProperty { get; set; } = new();
}

public class OccupancyDetailDto
{
    public int Occupied { get; set; }
    public int Total { get; set; }
    public double Rate { get; set; }
}

public class OccupancyHistoryDto
{
    public string Month { get; set; } = string.Empty;
    public double Rate { get; set; }
}

public class OccupancyByPropertyDto
{
    public string PropertyName { get; set; } = string.Empty;
    public int Occupied { get; set; }
    public int Total { get; set; }
    public double Rate { get; set; }
}
