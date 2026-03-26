using Microsoft.EntityFrameworkCore;

using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Reports.Queries.GetRevenueReport;

public record GetRevenueReportQuery(
    string Period = "this_month", // this_month, last_month, this_quarter, this_year, custom
    DateTime? From = null,
    DateTime? To = null,
    Guid? PropertyId = null,
    string GroupBy = "month" // month, property, method
) : IRequest<RevenueReportDto>;

public class GetRevenueReportQueryHandler(IApplicationDbContext dbContext, ITenantContext tenantContext)
    : IRequestHandler<GetRevenueReportQuery, RevenueReportDto>
{
    public async Task<RevenueReportDto> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var tenantId = tenantContext.TenantId;
        var (fromDate, toDate) = GetDateRange(request.Period, request.From, request.To);
        
        var report = new RevenueReportDto
        {
            Period = new PeriodDto { From = fromDate.ToString("yyyy-MM-dd"), To = toDate.ToString("yyyy-MM-dd") }
        };

        // 1. Summary
        const string summarySql = @"
            SELECT 
                COALESCE(SUM(amount), 0) as TotalRevenue,
                COUNT(*) as Count
            FROM transactions
            WHERE tenant_id = @tenantId AND direction = 'income' AND is_deleted = false
            AND paid_at >= @fromDate AND paid_at <= @toDate
            AND (@propertyId IS NULL OR property_id = @propertyId)";

        var summary = await connection.QuerySingleOrDefaultAsync<dynamic>(summarySql, new { tenantId, fromDate, toDate, propertyId = request.PropertyId });
        report.Summary.TotalRevenue = summary.totalrevenue;

        // Collection Rate (Simplified: Collected vs Invoiced in period)
        const string invoiceSql = @"
            SELECT COALESCE(SUM(total_amount), 0) FROM invoices
            WHERE tenant_id = @tenantId AND is_deleted = false
            AND created_at >= @fromDate AND created_at <= @toDate
            AND (@propertyId IS NULL OR contract_id IN (SELECT id FROM contracts WHERE property_id = @propertyId))";
        
        var totalInvoiced = await connection.ExecuteScalarAsync<decimal>(invoiceSql, new { tenantId, fromDate, toDate, propertyId = request.PropertyId });
        if (totalInvoiced > 0)
        {
            report.Summary.CollectionRate = Math.Round((double)report.Summary.TotalRevenue / (double)totalInvoiced * 100, 1);
        }

        // 2. By Month
        const string byMonthSql = @"
            SELECT 
                TO_CHAR(paid_at, 'YYYY-MM') as Month,
                SUM(amount) as Collected
            FROM transactions
            WHERE tenant_id = @tenantId AND direction = 'income' AND is_deleted = false
            AND paid_at >= @fromDate AND paid_at <= @toDate
            AND (@propertyId IS NULL OR property_id = @propertyId)
            GROUP BY TO_CHAR(paid_at, 'YYYY-MM')
            ORDER BY Month";
        
        var byMonth = await connection.QueryAsync<RevenueByMonthDto>(byMonthSql, new { tenantId, fromDate, toDate, propertyId = request.PropertyId });
        report.ByMonth = byMonth.ToList();

        // 3. By Property
        const string byPropertySql = @"
            SELECT 
                p.name as PropertyName,
                SUM(t.amount) as Collected
            FROM transactions t
            JOIN properties p ON t.property_id = p.id
            WHERE t.tenant_id = @tenantId AND t.direction = 'income' AND t.is_deleted = false
            AND t.paid_at >= @fromDate AND t.paid_at <= @toDate
            AND (@propertyId IS NULL OR t.property_id = @propertyId)
            GROUP BY p.name";
        
        var byProperty = await connection.QueryAsync<RevenueByPropertyDto>(byPropertySql, new { tenantId, fromDate, toDate, propertyId = request.PropertyId });
        report.ByProperty = byProperty.ToList();

        // 4. By Method
        const string byMethodSql = @"
            SELECT 
                method as Key,
                SUM(amount) as Value
            FROM transactions
            WHERE tenant_id = @tenantId AND direction = 'income' AND is_deleted = false
            AND paid_at >= @fromDate AND paid_at <= @toDate
            AND (@propertyId IS NULL OR property_id = @propertyId)
            GROUP BY method";
        
        var byMethod = await connection.QueryAsync<KeyValuePair<string, decimal>>(byMethodSql, new { tenantId, fromDate, toDate, propertyId = request.PropertyId });
        report.ByMethod = byMethod.ToDictionary(x => x.Key, x => x.Value);

        return report;
    }

    private (DateTime From, DateTime To) GetDateRange(string period, DateTime? from, DateTime? to)
    {
        var now = DateTime.Today;
        return period switch
        {
            "this_month" => (new DateTime(now.Year, now.Month, 1), now.AddDays(1).AddSeconds(-1)),
            "last_month" => (new DateTime(now.Year, now.Month, 1).AddMonths(-1), new DateTime(now.Year, now.Month, 1).AddSeconds(-1)),
            "this_quarter" => (new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1), now.AddDays(1).AddSeconds(-1)),
            "this_year" => (new DateTime(now.Year, 1, 1), now.AddDays(1).AddSeconds(-1)),
            "custom" => (from ?? now.AddMonths(-1), to ?? now.AddDays(1).AddSeconds(-1)),
            _ => (new DateTime(now.Year, now.Month, 1), now.AddDays(1).AddSeconds(-1))
        };
    }
}
