using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.Reports.Queries.GetDashboardSummary;
using RentalOS.Application.Modules.Reports.Queries.GetRevenueReport;
using RentalOS.Application.Modules.Reports.Queries.GetOccupancyReport;
using RentalOS.Application.Modules.Reports.Queries.GetCollectionRateReport;
using RentalOS.Application.Modules.Reports.Queries.GetOverdueTrend;
using RentalOS.Application.Modules.Reports.Queries.GetTopRooms;
using RentalOS.Application.Modules.Reports.Queries.GetMonthlySummary;
using RentalOS.Application.Modules.Reports.Queries.ExportReport;

namespace RentalOS.WebAPI.Controllers;

[Authorize(Roles = "owner,manager")]
[ApiController]
[Route("api/v1/[controller]")]
public class ReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboard()
    {
        return Ok(await mediator.Send(new GetDashboardSummaryQuery()));
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueReportDto>> GetRevenue([FromQuery] GetRevenueReportQuery query)
    {
        return Ok(await mediator.Send(query));
    }

    [HttpGet("occupancy")]
    public async Task<ActionResult<OccupancyReportDto>> GetOccupancy([FromQuery] Guid? propertyId)
    {
        return Ok(await mediator.Send(new GetOccupancyReportQuery(propertyId)));
    }

    [HttpGet("collection-rate")]
    public async Task<ActionResult<CollectionRateDto>> GetCollectionRate([FromQuery] GetCollectionRateReportQuery query)
    {
        return Ok(await mediator.Send(query));
    }

    [HttpGet("overdue-trend")]
    public async Task<ActionResult<List<OverdueTrendDto>>> GetOverdueTrend([FromQuery] int months = 6)
    {
        return Ok(await mediator.Send(new GetOverdueTrendQuery(months)));
    }

    [HttpGet("top-rooms")]
    public async Task<ActionResult<List<TopRoomDto>>> GetTopRooms([FromQuery] int top = 10)
    {
        return Ok(await mediator.Send(new GetTopRoomsQuery(top)));
    }

    [HttpGet("monthly-summary/{month}")]
    public async Task<ActionResult<MonthlySummaryDto>> GetMonthlySummary(string month)
    {
        return Ok(await mediator.Send(new GetMonthlySummaryQuery(month)));
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ExportReportQuery query)
    {
        var result = await mediator.Send(query);
        return File(result.Content, result.ContentType, result.FileName);
    }
}
