using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Rooms.Commands.DeleteRoom;

public record DeleteRoomCommand(Guid Id) : IRequest<Unit>;

public class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteRoomCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Rooms
            .Include(r => r.Contracts)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new Exception("ROOM_NOT_FOUND");
        }

        // Logic from Properties: Chỉ được xóa nếu không có phòng đang thuê
        // Spec for Rooms also implies consistency. 
        // Technically if rented, it's safer to block.
        if (entity.Status == Domain.Enums.RoomStatus.Rented)
        {
            throw new Exception("CANNOT_DELETE_RENTED_ROOM");
        }

        entity.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
