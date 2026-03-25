using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.MeterReadings.Commands.UpdateMeterReading;

public record UpdateMeterReadingCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public DateTime ReadingDate { get; init; }
    public int ElectricityReading { get; init; }
    public int WaterReading { get; init; }
    public string? ElectricityImage { get; init; }
    public string? WaterImage { get; init; }
    public string? Note { get; init; }
}

public class UpdateMeterReadingCommandHandler : IRequestHandler<UpdateMeterReadingCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateMeterReadingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateMeterReadingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MeterReadings
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new Exception("METER_READING_NOT_FOUND");
        }

        entity.ReadingDate = DateOnly.FromDateTime(request.ReadingDate);
        entity.ElectricityReading = request.ElectricityReading;
        entity.WaterReading = request.WaterReading;
        entity.ElectricityImage = request.ElectricityImage;
        entity.WaterImage = request.WaterImage;
        entity.Note = request.Note;

        // Logic: Tự động tính toán lại nếu có invoice pending tháng đó.
        var billingMonth = new DateOnly(request.ReadingDate.Year, request.ReadingDate.Month, 1);

        var invoice = await _context.Invoices
            .Include(i => i.Contract)
            .FirstOrDefaultAsync(i => i.Contract.RoomId == entity.RoomId 
                                 && i.BillingMonth == billingMonth 
                                 && i.Status == Domain.Enums.InvoiceStatus.Pending, cancellationToken);

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

        return Unit.Value;
    }
}
