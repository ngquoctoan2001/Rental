using System.Data;
using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Reports.Queries.GetCollectionRateReport;

public record GetCollectionRateReportQuery(Guid? PropertyId, DateTime? From, DateTime? To) : IRequest<CollectionRateDto>;

public class GetCollectionRateReportQueryHandler(IDbConnection dbConnection, ITenantContext tenantContext)
    : IRequestHandler<GetCollectionRateReportQuery, CollectionRateDto>
{
    public async Task<CollectionRateDto> Handle(GetCollectionRateReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var fromDate = request.From ?? DateTime.Today.AddMonths(-6);
        var toDate = request.To ?? DateTime.Today.AddDays(1).AddSeconds(-1);

        const string sql = @"
            WITH InvoiceStats AS (
                SELECT 
                    TO_CHAR(created_at, 'YYYY-MM') as Month,
                    SUM(total_amount) as Invoiced
                FROM invoices
                WHERE tenant_id = @tenantId AND is_deleted = false
                AND created_at >= @fromDate AND created_at <= @toDate
                AND (@propertyId IS NULL OR contract_id IN (SELECT id FROM contracts WHERE property_id = @propertyId))
                GROUP BY Month
            ),
            TransactionStats AS (
                SELECT 
                    TO_CHAR(paid_at, 'YYYY-MM') as Month,
                    SUM(amount) as Collected
                FROM transactions
                WHERE tenant_id = @tenantId AND direction = 'income' AND is_deleted = false
                AND paid_at >= @fromDate AND paid_at <= @toDate
                AND (@propertyId IS NULL OR property_id = @propertyId)
                GROUP BY Month
            )
            SELECT 
                i.Month,
                COALESCE(i.Invoiced, 0) as Invoiced,
                COALESCE(t.Collected, 0) as Collected
            FROM InvoiceStats i
            LEFT JOIN TransactionStats t ON i.Month = t.Month
            ORDER BY i.Month";

        var details = await dbConnection.QueryAsync<CollectionRateByMonthDto>(sql, new { tenantId, fromDate, toDate, propertyId = request.PropertyId });
        
        var result = new CollectionRateDto();
        foreach (var item in details)
        {
            if (item.Invoiced > 0) item.Rate = Math.Round((double)item.Collected / (double)item.Invoiced * 100, 1);
            result.TotalInvoiced += item.Invoiced;
            result.TotalCollected += item.Collected;
        }
        
        result.Details = details.ToList();
        if (result.TotalInvoiced > 0)
        {
            result.Rate = Math.Round((double)result.TotalCollected / (double)result.TotalInvoiced * 100, 1);
        }

        return result;
    }
}
