using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.Rooms.Dtos;

namespace RentalOS.Application.Modules.Rooms.Queries.GetRoomHistory;

public record GetRoomHistoryQuery(Guid Id) : IRequest<RoomHistoryDto>;

public class GetRoomHistoryQueryHandler : IRequestHandler<GetRoomHistoryQuery, RoomHistoryDto>
{
    private readonly IApplicationDbContext _context;

    public GetRoomHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoomHistoryDto> Handle(GetRoomHistoryQuery request, CancellationToken cancellationToken)
    {
        var room = await _context.Rooms
            .Include(r => r.Contracts)
                .ThenInclude(c => c.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (room == null)
        {
            throw new Exception("ROOM_NOT_FOUND");
        }

        var history = new RoomHistoryDto
        {
            Contracts = room.Contracts
                .OrderByDescending(c => c.StartDate)
                .Select(c => new ContractSummaryDto
                {
                    Id = c.Id,
                    ContractCode = c.ContractCode,
                    CustomerName = c.Customer.FullName,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status
                })
                .ToList(),
            
            // Maintenance logs could come from a separate table if it exists.
            // For now, if current status is maintenance, we could show current log.
            // But usually history implies past events. If there's no Separate MaintenanceLog table, 
            // we skip or use Audit logs if implemented. 
            // I'll leave it empty for now as requested by spec "GetRoomHistory".
            MaintenanceLogs = [] 
        };

        return history;
    }
}
