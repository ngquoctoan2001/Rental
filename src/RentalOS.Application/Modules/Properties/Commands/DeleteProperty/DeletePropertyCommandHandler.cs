using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Properties.Commands.DeleteProperty;

public class DeletePropertyCommandHandler(IApplicationDbContext context) 
    : IRequestHandler<DeletePropertyCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await context.Properties
            .Include(p => p.Rooms)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (property == null)
        {
            return Result<bool>.Fail("PROPERTY_NOT_FOUND", "Không tìm thấy nhà trọ.");
        }

        // Kiểm tra xem có phòng nào đang thuê không
        var hasRentedRooms = property.Rooms.Any(r => r.Status == RoomStatus.Rented && r.IsActive);
        if (hasRentedRooms)
        {
            return Result<bool>.Fail("PROPERTY_HAS_RENTED_ROOMS", "Không thể xóa nhà trọ đang có phòng được thuê.");
        }

        property.IsActive = false; // Soft delete
        
        // Disable all rooms too? Usually yes.
        foreach (var room in property.Rooms)
        {
            room.IsActive = false;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
