using Microsoft.EntityFrameworkCore;

using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Reports.Queries.GetOverdueTrend;

public record GetOverdueTrendQuery(int Months = 6) : IRequest<List<OverdueTrendDto>>;

public class GetOverdueTrendQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetOverdueTrendQuery, List<OverdueTrendDto>>
{
    public async Task<List<OverdueTrendDto>> Handle(GetOverdueTrendQuery request, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await dbContext.Database.OpenConnectionAsync(cancellationToken);

        var fromDate = DateTime.Today.AddMonths(-request.Months);

        const string sql = @"
            SELECT 
                TO_CHAR(due_date, 'YYYY-MM') as Month,
                COUNT(*) as OverdueCount,
                SUM(total_amount - COALESCE(partial_paid_amount, 0)) as OverdueAmount
            FROM invoices
            WHERE status = 'Overdue'
            AND due_date >= @fromDate
            GROUP BY TO_CHAR(due_date, 'YYYY-MM')
            ORDER BY TO_CHAR(due_date, 'YYYY-MM')";

        var result = await connection.QueryAsync<OverdueTrendDto>(sql, new { fromDate });
        return result.ToList();
    }
}
