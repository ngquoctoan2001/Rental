using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalOS.Infrastructure.Persistence;

namespace RentalOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ReportsController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>Returns key dashboard statistics for the current tenant.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        // Use raw SQL to match tenant schema DDL (snake_case columns, string enums)
        // Open via EF Core API to trigger TenantConnectionInterceptor (sets search_path)
        if (_db.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
            await _db.Database.OpenConnectionAsync(ct);
        var conn = _db.Database.GetDbConnection();

        var sql = """
            SELECT
                (SELECT COUNT(*) FROM rooms)::int                                               AS total_rooms,
                (SELECT COUNT(*) FROM rooms WHERE status = 'available')::int                   AS available_rooms,
                (SELECT COUNT(*) FROM contracts WHERE status = 'active')::int                  AS total_tenants,
                COALESCE(
                    (SELECT SUM(amount) FROM transactions
                     WHERE paid_at >= date_trunc('month', NOW() AT TIME ZONE 'UTC')),
                    0
                )                                                                               AS monthly_revenue
            """;

#pragma warning disable DAP005
        var result = await conn.QueryFirstOrDefaultAsync(sql);
#pragma warning restore DAP005

        return Ok(new
        {
            totalRooms     = Convert.ToInt32(result?.total_rooms ?? 0),
            availableRooms = Convert.ToInt32(result?.available_rooms ?? 0),
            totalTenants   = Convert.ToInt32(result?.total_tenants ?? 0),
            monthlyRevenue = Convert.ToDecimal(result?.monthly_revenue ?? 0),
        });
    }
}
