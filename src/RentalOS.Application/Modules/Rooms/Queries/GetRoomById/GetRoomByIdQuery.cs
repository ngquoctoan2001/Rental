using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.Rooms.Dtos;
using System.Text.Json;

namespace RentalOS.Application.Modules.Rooms.Queries.GetRoomById;

public record GetRoomByIdQuery(Guid Id) : IRequest<RoomDto>;

public class GetRoomByIdQueryHandler : IRequestHandler<GetRoomByIdQuery, RoomDto>
{
    private readonly IApplicationDbContext _context;

    public GetRoomByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoomDto> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _context.Rooms
            .Include(r => r.Property)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (r == null || !r.IsActive)
        {
            throw new Exception("ROOM_NOT_FOUND");
        }

        return new RoomDto
        {
            Id = r.Id,
            PropertyId = r.PropertyId,
            PropertyName = r.Property.Name,
            RoomNumber = r.RoomNumber,
            Floor = r.Floor,
            AreaSqm = r.AreaSqm,
            BasePrice = r.BasePrice,
            ElectricityPrice = r.ElectricityPrice,
            WaterPrice = r.WaterPrice,
            ServiceFee = r.ServiceFee,
            InternetFee = r.InternetFee,
            GarbageFee = r.GarbageFee,
            Status = r.Status,
            Amenities = string.IsNullOrEmpty(r.Amenities) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(r.Amenities)!,
            Images = string.IsNullOrEmpty(r.Images) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(r.Images)!,
            Notes = r.Notes,
            MaintenanceNote = r.MaintenanceNote,
            MaintenanceSince = r.MaintenanceSince
        };
    }
}
