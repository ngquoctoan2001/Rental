using Microsoft.EntityFrameworkCore;

using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Reports.Queries.GetDashboardSummary;

public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;

public class GetDashboardSummaryQueryHandler(IApplicationDbContext dbContext, ITenantContext tenantContext) 
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var tenantId = tenantContext.TenantId;
        var dashboard = new DashboardSummaryDto();

        // 1. Room Stats
        const string roomSql = @"
            SELECT 
                COUNT(*) as Total,
                COUNT(*) FILTER (WHERE status = 'available') as Available,
                COUNT(*) FILTER (WHERE status = 'rented') as Rented,
                COUNT(*) FILTER (WHERE status = 'maintenance') as Maintenance
            FROM rooms 
            WHERE tenant_id = @tenantId AND is_deleted = false";
        
        var roomStats = await connection.QuerySingleOrDefaultAsync<RoomStatsDto>(roomSql, new { tenantId });
        if (roomStats != null)
        {
            dashboard.Rooms = roomStats;
            if (roomStats.Total > 0)
            {
                dashboard.Rooms.OccupancyRate = Math.Round((double)roomStats.Rented / roomStats.Total * 100, 1);
            }
        }

        // 2. Revenue Stats (This month vs Last month)
        var today = DateTime.Today;
        var firstDayThisMonth = new DateTime(today.Year, today.Month, 1);
        var firstDayLastMonth = firstDayThisMonth.AddMonths(-1);
        var lastDayLastMonth = firstDayThisMonth.AddDays(-1);

        const string revenueSql = @"
            SELECT 
                COALESCE(SUM(CASE WHEN paid_at >= @firstDayThisMonth THEN amount ELSE 0 END), 0) as ThisMonth,
                COALESCE(SUM(CASE WHEN paid_at >= @firstDayLastMonth AND paid_at <= @lastDayLastMonth THEN amount ELSE 0 END), 0) as LastMonth
            FROM transactions 
            WHERE tenant_id = @tenantId AND direction = 'income' AND is_deleted = false";

        var revStats = await connection.QuerySingleOrDefaultAsync<RevenueStatsDto>(revenueSql, 
            new { tenantId, firstDayThisMonth, firstDayLastMonth, lastDayLastMonth });
        
        if (revStats != null)
        {
            dashboard.Revenue = revStats;
            if (revStats.LastMonth > 0)
            {
                dashboard.Revenue.ChangePercent = (double)Math.Round((revStats.ThisMonth - revStats.LastMonth) / revStats.LastMonth * 100, 1);
            }
        }

        // 3. Invoice Stats
        const string invoiceSql = @"
            SELECT 
                COUNT(*) FILTER (WHERE status IN ('pending', 'overdue')) as PendingCount,
                COUNT(*) FILTER (WHERE status = 'overdue') as OverdueCount,
                COALESCE(SUM(CASE WHEN status IN ('pending', 'overdue') THEN total_amount - paid_amount ELSE 0 END), 0) as PendingAmount,
                COALESCE(SUM(CASE WHEN status = 'overdue' THEN total_amount - paid_amount ELSE 0 END), 0) as OverdueAmount
            FROM invoices 
            WHERE tenant_id = @tenantId AND is_deleted = false";

        var invStats = await connection.QuerySingleOrDefaultAsync<InvoiceStatsDto>(invoiceSql, new { tenantId });
        if (invStats != null) dashboard.Invoices = invStats;

        // 4. Contract Stats
        var date30 = today.AddDays(30);
        var date7 = today.AddDays(7);

        const string contractSql = @"
            SELECT 
                COUNT(*) FILTER (WHERE end_date <= @date30 AND status = 'active') as ExpiringIn30Days,
                COUNT(*) FILTER (WHERE end_date <= @date7 AND status = 'active') as ExpiringIn7Days
            FROM contracts 
            WHERE tenant_id = @tenantId AND is_deleted = false";

        var conStats = await connection.QuerySingleOrDefaultAsync<ContractStatsDto>(contractSql, new { tenantId, date30, date7 });
        if (conStats != null) dashboard.Contracts = conStats;

        // 5. Build Alerts
        if (dashboard.Contracts.ExpiringIn30Days > 0)
        {
            dashboard.Alerts.Add(new AlertDto 
            { 
                Type = "contract_expiry", 
                Count = dashboard.Contracts.ExpiringIn30Days, 
                Message = $"{dashboard.Contracts.ExpiringIn30Days} hợp đồng hết hạn trong 30 ngày" 
            });
        }

        if (dashboard.Invoices.OverdueCount > 0)
        {
            dashboard.Alerts.Add(new AlertDto 
            { 
                Type = "overdue_invoice", 
                Count = dashboard.Invoices.OverdueCount, 
                Message = $"{dashboard.Invoices.OverdueCount} hóa đơn quá hạn chưa thu" 
            });
        }

        return dashboard;
    }
}
