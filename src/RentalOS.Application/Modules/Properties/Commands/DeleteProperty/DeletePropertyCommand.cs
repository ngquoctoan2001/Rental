using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Properties.Commands.DeleteProperty;

public record DeletePropertyCommand(Guid Id) : IRequest<Unit>;

public class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeletePropertyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Properties
            .Include(p => p.Rooms)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new Exception("PROPERTY_NOT_FOUND");
        }

        // Only delete if no rented rooms
        if (entity.Rooms.Any(r => r.Status == RoomStatus.Rented))
        {
            throw new Exception("PROPERTY_HAS_RENTED_ROOMS: Không thể xóa nhà trọ đang có phòng được thuê.");
        }

        entity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
