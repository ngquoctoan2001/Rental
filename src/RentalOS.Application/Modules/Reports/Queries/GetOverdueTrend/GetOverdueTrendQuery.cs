using Microsoft.EntityFrameworkCore;

using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Reports.Queries.GetOverdueTrend;

public record GetOverdueTrendQuery(int Months = 6) : IRequest<List<OverdueTrendDto>>;

public class GetOverdueTrendQueryHandler(IApplicationDbContext dbContext, ITenantContext tenantContext)
    : IRequestHandler<GetOverdueTrendQuery, List<OverdueTrendDto>>
{
    public async Task<List<OverdueTrendDto>> Handle(GetOverdueTrendQuery request, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var tenantId = tenantContext.TenantId;
        var fromDate = DateTime.Today.AddMonths(-request.Months);

        const string sql = @"
            SELECT 
                TO_CHAR(due_date, 'YYYY-MM') as Month,
                COUNT(*) as OverdueCount,
                SUM(total_amount - paid_amount) as OverdueAmount
            FROM invoices
            WHERE tenant_id = @tenantId AND status = 'overdue' AND is_deleted = false
            AND due_date >= @fromDate
            GROUP BY Month
            ORDER BY Month";

        var result = await connection.QueryAsync<OverdueTrendDto>(sql, new { tenantId, fromDate });
        return result.ToList();
    }
}
