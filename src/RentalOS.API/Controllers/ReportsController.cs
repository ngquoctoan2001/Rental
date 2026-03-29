using Dapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Modules.Reports.Queries.ExportReport;
using RentalOS.Application.Modules.Reports.Queries.GetCollectionRateReport;
using RentalOS.Application.Modules.Reports.Queries.GetMonthlySummary;
using RentalOS.Application.Modules.Reports.Queries.GetOccupancyReport;
using RentalOS.Application.Modules.Reports.Queries.GetOverdueTrend;
using RentalOS.Application.Modules.Reports.Queries.GetRevenueReport;
using RentalOS.Application.Modules.Reports.Queries.GetTopRooms;
using RentalOS.Infrastructure.Persistence;

namespace RentalOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ReportsController(ApplicationDbContext db, ISender sender) : ControllerBase
{
    /// <summary>Returns key dashboard statistics for the current tenant.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] Guid? propertyId = null, CancellationToken ct = default)
    {
        // Use raw SQL to match tenant schema DDL (snake_case columns, string enums)
        // Open via EF Core API to trigger TenantConnectionInterceptor (sets search_path)
        if (db.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
            await db.Database.OpenConnectionAsync(ct);
        var conn = db.Database.GetDbConnection();

        var sql = """
            SELECT
                (SELECT COUNT(*) FROM rooms WHERE (@propertyId IS NULL OR property_id = @propertyId))::int AS total_rooms,
                (SELECT COUNT(*) FROM rooms WHERE status = 'available' AND (@propertyId IS NULL OR property_id = @propertyId))::int AS available_rooms,
                (SELECT COUNT(*) FROM contracts c
                 INNER JOIN rooms r ON r.id = c.room_id
                 WHERE c.status = 'active' AND (@propertyId IS NULL OR r.property_id = @propertyId))::int AS total_tenants,
                COALESCE(
                    (SELECT SUM(t.amount) FROM transactions t
                     LEFT JOIN invoices i ON i.id = t.invoice_id
                     LEFT JOIN contracts c ON c.id = i.contract_id
                     LEFT JOIN rooms r ON r.id = c.room_id
                     WHERE t.paid_at >= date_trunc('month', NOW() AT TIME ZONE 'UTC')
                       AND (@propertyId IS NULL OR r.property_id = @propertyId)),
                    0
                )                                                                               AS monthly_revenue
            """;

#pragma warning disable DAP005
        var result = await conn.QueryFirstOrDefaultAsync(sql, new { propertyId });
#pragma warning restore DAP005

        return Ok(new
        {
            totalRooms     = Convert.ToInt32(result?.total_rooms ?? 0),
            availableRooms = Convert.ToInt32(result?.available_rooms ?? 0),
            totalTenants   = Convert.ToInt32(result?.total_tenants ?? 0),
            monthlyRevenue = Convert.ToDecimal(result?.monthly_revenue ?? 0),
        });
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] string period = "this_month",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] Guid? propertyId = null,
        [FromQuery] string groupBy = "month",
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetRevenueReportQuery(period, from, to, propertyId, groupBy), ct));

    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancy(
        [FromQuery] Guid? propertyId = null,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetOccupancyReportQuery(propertyId), ct));

    [HttpGet("collection-rate")]
    public async Task<IActionResult> GetCollectionRate(
        [FromQuery] Guid? propertyId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetCollectionRateReportQuery(propertyId, from, to), ct));

    [HttpGet("export/{type}")]
    public async Task<IActionResult> Export(
        string type,
        [FromQuery] string period = "this_month",
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ExportReportQuery(type, period), ct);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlySummary(
        [FromQuery] string month,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetMonthlySummaryQuery(month), ct));

    [HttpGet("overdue-trend")]
    public async Task<IActionResult> GetOverdueTrend(
        [FromQuery] int months = 6,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetOverdueTrendQuery(months), ct));

    [HttpGet("top-rooms")]
    public async Task<IActionResult> GetTopRooms(
        [FromQuery] int top = 10,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetTopRoomsQuery(top), ct));
}
