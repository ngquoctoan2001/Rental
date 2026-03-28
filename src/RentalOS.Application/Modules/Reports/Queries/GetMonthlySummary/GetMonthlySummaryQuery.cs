using Microsoft.EntityFrameworkCore;

using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Reports.Queries.GetMonthlySummary;

public record GetMonthlySummaryQuery(string Month) : IRequest<MonthlySummaryDto>;

public class MonthlySummaryDto
{
    public string Month { get; set; } = string.Empty;
    public decimal TotalInvoiced { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int NewContracts { get; set; }
    public int TerminatedContracts { get; set; }
}

public class GetMonthlySummaryQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMonthlySummaryQuery, MonthlySummaryDto>
{
    public async Task<MonthlySummaryDto> Handle(GetMonthlySummaryQuery request, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await dbContext.Database.OpenConnectionAsync(cancellationToken);

        var month = request.Month; // YYYY-MM

        const string sql = @"
            SELECT 
                @month as Month,
                (SELECT COALESCE(SUM(total_amount), 0) FROM invoices WHERE TO_CHAR(created_at, 'YYYY-MM') = @month) as TotalInvoiced,
                (SELECT COALESCE(SUM(amount), 0) FROM transactions WHERE TO_CHAR(paid_at, 'YYYY-MM') = @month AND direction = 'Income') as TotalCollected,
                (SELECT COALESCE(SUM(total_amount - COALESCE(partial_paid_amount, 0)), 0) FROM invoices WHERE status = 'Overdue') as OutstandingAmount,
                (SELECT COUNT(*) FROM contracts WHERE TO_CHAR(created_at, 'YYYY-MM') = @month) as NewContracts,
                (SELECT COUNT(*) FROM contracts WHERE TO_CHAR(updated_at, 'YYYY-MM') = @month AND status = 'Terminated') as TerminatedContracts
        ";

        return await connection.QuerySingleAsync<MonthlySummaryDto>(sql, new { month });
    }
}
