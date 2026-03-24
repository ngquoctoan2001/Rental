using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Rooms.Commands.ChangeRoomStatus;

public record ChangeRoomStatusCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public RoomStatus NewStatus { get; init; }
    public string? MaintenanceNote { get; init; }
    public DateOnly? EstimatedDoneDate { get; init; } // Actually spec mentions maintenanceSince and maintenanceNote
}

public class ChangeRoomStatusCommandHandler : IRequestHandler<ChangeRoomStatusCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public ChangeRoomStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ChangeRoomStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new Exception("ROOM_NOT_FOUND");
        }

        // available -> maintenance: set MaintenanceNote, MaintenanceSince = today
        // maintenance -> available: clear MaintenanceNote, MaintenanceSince
        // Không được đổi status phòng đang rented về available trực tiếp (phải qua terminate contract)

        if (entity.Status == RoomStatus.Rented && request.NewStatus == RoomStatus.Available)
        {
            throw new Exception("CANNOT_SET_RENTED_TO_AVAILABLE_DIRECTLY");
        }

        if (request.NewStatus == RoomStatus.Maintenance)
        {
            entity.MaintenanceNote = request.MaintenanceNote;
            entity.MaintenanceSince = DateOnly.FromDateTime(DateTime.UtcNow);
        }
        else if (request.NewStatus == RoomStatus.Available)
        {
            entity.MaintenanceNote = null;
            entity.MaintenanceSince = null;
        }

        entity.Status = request.NewStatus;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
