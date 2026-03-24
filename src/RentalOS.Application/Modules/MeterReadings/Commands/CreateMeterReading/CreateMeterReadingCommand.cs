using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.MeterReadings.Commands.CreateMeterReading;

public record CreateMeterReadingCommand : IRequest<Guid>
{
    public Guid RoomId { get; init; }
    public DateTime ReadingDate { get; init; }
    public int ElectricityReading { get; init; }
    public int WaterReading { get; init; }
    public string? ElectricityImage { get; init; }
    public string? WaterImage { get; init; }
    public string? Note { get; init; }
}

public class CreateMeterReadingCommandHandler : IRequestHandler<CreateMeterReadingCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateMeterReadingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateMeterReadingCommand request, CancellationToken cancellationToken)
    {
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken);

        if (room == null)
        {
            throw new Exception("ROOM_NOT_FOUND");
        }

        var entity = new MeterReading
        {
            RoomId = request.RoomId,
            ReadingDate = DateOnly.FromDateTime(request.ReadingDate),
            ElectricityReading = request.ElectricityReading,
            WaterReading = request.WaterReading,
            ElectricityImage = request.ElectricityImage,
            WaterImage = request.WaterImage,
            Note = request.Note
        };

        _context.MeterReadings.Add(entity);

        // Logic: Tự động tính toán nếu có invoice pending tháng đó.
        // BillingMonth of Invoice is first day of the month.
        var billingMonth = new DateOnly(request.ReadingDate.Year, request.ReadingDate.Month, 1);

        var invoice = await _context.Invoices
            .Include(i => i.Contract)
            .FirstOrDefaultAsync(i => i.Contract.RoomId == request.RoomId 
                                 && i.BillingMonth == billingMonth 
                                 && i.Status == InvoiceStatus.Pending, cancellationToken);

        if (invoice != null)
        {
            invoice.ElectricityNew = request.ElectricityReading;
            invoice.WaterNew = request.WaterReading;
            invoice.MeterImageElectricity = request.ElectricityImage;
            invoice.MeterImageWater = request.WaterImage;
            
            // Re-calculate totals
            invoice.ElectricityAmount = (invoice.ElectricityNew - invoice.ElectricityOld) * invoice.ElectricityPrice;
            invoice.WaterAmount = (invoice.WaterNew - invoice.WaterOld) * invoice.WaterPrice;
            
            invoice.TotalAmount = invoice.RoomRent + invoice.ElectricityAmount + invoice.WaterAmount + 
                                  invoice.ServiceFee + invoice.InternetFee + invoice.GarbageFee + 
                                  invoice.OtherFees - invoice.Discount;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
