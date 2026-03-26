using Microsoft.EntityFrameworkCore;

using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Reports.Queries.GetOccupancyReport;

public record GetOccupancyReportQuery(Guid? PropertyId) : IRequest<OccupancyReportDto>;

public class GetOccupancyReportQueryHandler(IApplicationDbContext dbContext, ITenantContext tenantContext)
    : IRequestHandler<GetOccupancyReportQuery, OccupancyReportDto>
{
    public async Task<OccupancyReportDto> Handle(GetOccupancyReportQuery request, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var tenantId = tenantContext.TenantId;
        var report = new OccupancyReportDto();

        // 1. Current Occupancy
        const string currentSql = @"
            SELECT 
                COUNT(*) FILTER (WHERE status = 'rented') as Occupied,
                COUNT(*) as Total
            FROM rooms 
            WHERE tenant_id = @tenantId AND is_deleted = false
            AND (@propertyId IS NULL OR property_id = @propertyId)";

        var current = await connection.QuerySingleOrDefaultAsync<OccupancyDetailDto>(currentSql, new { tenantId, request.PropertyId });
        if (current != null)
        {
            report.Current = current;
            if (current.Total > 0) report.Current.Rate = Math.Round((double)current.Occupied / current.Total * 100, 1);
        }

        // 2. Occupancy by Property (if not specific property)
        if (request.PropertyId == null)
        {
            const string byPropertySql = @"
                SELECT 
                    p.name as PropertyName,
                    COUNT(*) FILTER (WHERE r.status = 'rented') as Occupied,
                    COUNT(*) as Total
                FROM properties p
                JOIN rooms r ON p.id = r.property_id
                WHERE p.tenant_id = @tenantId AND p.is_deleted = false AND r.is_deleted = false
                GROUP BY p.name";
            
            var byProperty = await connection.QueryAsync<OccupancyByPropertyDto>(byPropertySql, new { tenantId });
            foreach (var prop in byProperty)
            {
                if (prop.Total > 0) prop.Rate = Math.Round((double)prop.Occupied / prop.Total * 100, 1);
            }
            report.ByProperty = byProperty.ToList();
        }

        // 3. History (Mock for now or complicated SQL)
        // In real app, we need a room_history table. For now, we take from current as snapshots.
        report.History = new List<OccupancyHistoryDto> 
        { 
            new() { Month = DateTime.Now.ToString("yyyy-MM"), Rate = report.Current.Rate } 
        };

        return report;
    }
}
