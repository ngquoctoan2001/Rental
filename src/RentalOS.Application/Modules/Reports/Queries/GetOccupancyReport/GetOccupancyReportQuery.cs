using Microsoft.EntityFrameworkCore;

using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Reports.Queries.GetOccupancyReport;

public record GetOccupancyReportQuery(Guid? PropertyId) : IRequest<OccupancyReportDto>;

public class GetOccupancyReportQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetOccupancyReportQuery, OccupancyReportDto>
{
    public async Task<OccupancyReportDto> Handle(GetOccupancyReportQuery request, CancellationToken cancellationToken)
    {
        if (dbContext.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
            await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();
        var report = new OccupancyReportDto();

        // 1. Current Occupancy
        const string currentSql = @"
            SELECT 
                COUNT(*) FILTER (WHERE status = 'Rented') as Occupied,
                COUNT(*) as Total
            FROM rooms 
            WHERE (@propertyId IS NULL OR property_id = @propertyId)";

        var current = await connection.QuerySingleOrDefaultAsync<OccupancyDetailDto>(currentSql, new { request.PropertyId });
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
                    COUNT(*) FILTER (WHERE r.status = 'Rented') as Occupied,
                    COUNT(*) as Total
                FROM properties p
                JOIN rooms r ON p.id = r.property_id
                GROUP BY p.name";
            
            var byProperty = await connection.QueryAsync<OccupancyByPropertyDto>(byPropertySql, new { });
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
