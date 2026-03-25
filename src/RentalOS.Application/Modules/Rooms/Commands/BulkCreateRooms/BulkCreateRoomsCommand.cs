using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;
using System.Text.Json;

namespace RentalOS.Application.Modules.Rooms.Commands.BulkCreateRooms;

public record BulkCreateRoomsCommand : IRequest<List<Guid>>
{
    public Guid PropertyId { get; init; }
    public List<BulkRoomItem> Rooms { get; init; } = [];
}

public record BulkRoomItem
{
    public string RoomNumber { get; init; } = string.Empty;
    public int Floor { get; init; } = 1;
    public decimal BasePrice { get; init; }
    public decimal? AreaSqm { get; init; }
    public decimal ElectricityPrice { get; init; } = 3500;
    public decimal WaterPrice { get; init; } = 15000;
}

public class BulkCreateRoomsCommandHandler : IRequestHandler<BulkCreateRoomsCommand, List<Guid>>
{
    private readonly IApplicationDbContext _context;

    public BulkCreateRoomsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Guid>> Handle(BulkCreateRoomsCommand request, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken);

        if (property == null)
        {
            throw new Exception("PROPERTY_NOT_FOUND");
        }

        var ids = new List<Guid>();
        
        // Use manual transaction if needed, but DbContext.SaveChangesAsync is atomic usually.
        // Spec says "transaction, rollback if error".
        
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var item in request.Rooms)
            {
                var entity = new Room
                {
                    PropertyId = request.PropertyId,
                    RoomNumber = item.RoomNumber,
                    Floor = item.Floor,
                    BasePrice = item.BasePrice,
                    AreaSqm = item.AreaSqm,
                    ElectricityPrice = item.ElectricityPrice,
                    WaterPrice = item.WaterPrice,
                    Status = Domain.Enums.RoomStatus.Available,
                    Amenities = "[]",
                    Images = "[]"
                };
                
                _context.Rooms.Add(entity);
                ids.Add(entity.Id);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return ids;
    }
}
