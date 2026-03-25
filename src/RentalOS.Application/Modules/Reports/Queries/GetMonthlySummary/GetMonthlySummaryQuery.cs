using System.Data;
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

public class GetMonthlySummaryQueryHandler(IDbConnection dbConnection, ITenantContext tenantContext)
    : IRequestHandler<GetMonthlySummaryQuery, MonthlySummaryDto>
{
    public async Task<MonthlySummaryDto> Handle(GetMonthlySummaryQuery request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var month = request.Month; // YYYY-MM

        const string sql = @"
            SELECT 
                @month as Month,
                (SELECT COALESCE(SUM(total_amount), 0) FROM invoices WHERE tenant_id = @tenantId AND TO_CHAR(created_at, 'YYYY-MM') = @month AND is_deleted = false) as TotalInvoiced,
                (SELECT COALESCE(SUM(amount), 0) FROM transactions WHERE tenant_id = @tenantId AND TO_CHAR(paid_at, 'YYYY-MM') = @month AND direction = 'income' AND is_deleted = false) as TotalCollected,
                (SELECT COALESCE(SUM(total_amount - paid_amount), 0) FROM invoices WHERE tenant_id = @tenantId AND status = 'overdue' AND is_deleted = false) as OutstandingAmount,
                (SELECT COUNT(*) FROM contracts WHERE tenant_id = @tenantId AND TO_CHAR(created_at, 'YYYY-MM') = @month AND is_deleted = false) as NewContracts,
                (SELECT COUNT(*) FROM contracts WHERE tenant_id = @tenantId AND TO_CHAR(updated_at, 'YYYY-MM') = @month AND status = 'terminated' AND is_deleted = false) as TerminatedContracts
        ";

        return await dbConnection.QuerySingleAsync<MonthlySummaryDto>(sql, new { tenantId, month });
    }
}
