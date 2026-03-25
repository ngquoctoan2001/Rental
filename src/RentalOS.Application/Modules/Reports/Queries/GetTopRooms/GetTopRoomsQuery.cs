using System.Data;
using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Reports.Queries.GetTopRooms;

public record GetTopRoomsQuery(int Top = 10) : IRequest<List<TopRoomDto>>;

public class GetTopRoomsQueryHandler(IDbConnection dbConnection, ITenantContext tenantContext)
    : IRequestHandler<GetTopRoomsQuery, List<TopRoomDto>>
{
    public async Task<List<TopRoomDto>> Handle(GetTopRoomsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;

        const string sql = @"
            SELECT 
                r.room_number as RoomNumber,
                p.name as PropertyName,
                SUM(t.amount) as TotalRevenue,
                COUNT(DISTINCT TO_CHAR(t.paid_at, 'YYYY-MM')) as OccupancyMonths
            FROM transactions t
            JOIN rooms r ON t.property_id = r.property_id -- This might need fixing if room_id is not in transaction directly
            JOIN properties p ON t.property_id = p.id
            WHERE t.tenant_id = @tenantId AND t.direction = 'income' AND t.is_deleted = false
            -- Note: in real schema, transactions should link to room or invoice
            -- Assuming room_number can be inferred or we link via invoices
            GROUP BY r.room_number, p.name
            ORDER BY TotalRevenue DESC
            LIMIT @top";
        
        // Wait, I should check transaction schema to see if it has RoomId or InvoiceId
        // From previous tasks, Transaction has InvoiceId
        const string correctSql = @"
            SELECT 
                r.room_number as RoomNumber,
                p.name as PropertyName,
                SUM(t.amount) as TotalRevenue,
                COUNT(DISTINCT TO_CHAR(t.paid_at, 'YYYY-MM')) as OccupancyMonths
            FROM transactions t
            LEFT JOIN invoices i ON t.invoice_id = i.id
            LEFT JOIN contracts c ON i.contract_id = c.id
            LEFT JOIN rooms r ON c.room_id = r.id
            LEFT JOIN properties p ON r.property_id = p.id
            WHERE t.tenant_id = @tenantId AND t.direction = 'income' AND t.is_deleted = false
            AND r.id IS NOT NULL
            GROUP BY r.room_number, p.name
            ORDER BY TotalRevenue DESC
            LIMIT @top";

        var result = await dbConnection.QueryAsync<TopRoomDto>(correctSql, new { tenantId, top = request.Top });
        return result.ToList();
    }
}
