using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.Rooms.Dtos;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Rooms.Queries.GetAvailableRooms;

public record GetAvailableRoomsQuery(Guid? PropertyId) : IRequest<List<RoomListItemDto>>;

public class GetAvailableRoomsQueryHandler : IRequestHandler<GetAvailableRoomsQuery, List<RoomListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableRoomsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoomListItemDto>> Handle(GetAvailableRoomsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Rooms
            .Include(r => r.Property)
            .Where(r => r.IsActive && r.Status == RoomStatus.Available)
            .AsNoTracking();

        if (request.PropertyId.HasValue)
            query = query.Where(r => r.PropertyId == request.PropertyId.Value);

        return await query
            .OrderBy(r => r.Property.Name)
            .ThenBy(r => r.RoomNumber)
            .Select(r => new RoomListItemDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Floor = r.Floor,
                BasePrice = r.BasePrice,
                Status = r.Status,
                PropertyName = r.Property.Name
            })
            .ToListAsync(cancellationToken);
    }
}
