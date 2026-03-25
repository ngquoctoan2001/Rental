using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.MeterReadings.Commands.DeleteMeterReading;

public record DeleteMeterReadingCommand(Guid Id) : IRequest<Unit>;

public class DeleteMeterReadingCommandHandler : IRequestHandler<DeleteMeterReadingCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteMeterReadingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteMeterReadingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MeterReadings
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new Exception("METER_READING_NOT_FOUND");
        }

        _context.MeterReadings.Remove(entity);

        // Logic: Reset invoice pending nếu reading bị xóa.
        var billingMonth = new DateOnly(entity.ReadingDate.Year, entity.ReadingDate.Month, 1);

        var invoice = await _context.Invoices
            .Include(i => i.Contract)
            .FirstOrDefaultAsync(i => i.Contract.RoomId == entity.RoomId 
                                 && i.BillingMonth == billingMonth 
                                 && i.Status == Domain.Enums.InvoiceStatus.Pending, cancellationToken);

        if (invoice != null)
        {
            // Reset về old (coi như chưa có chỉ số mới)
            invoice.ElectricityNew = invoice.ElectricityOld;
            invoice.WaterNew = invoice.WaterOld;
            invoice.MeterImageElectricity = null;
            invoice.MeterImageWater = null;
            
            invoice.ElectricityAmount = 0;
            invoice.WaterAmount = 0;
            
            invoice.TotalAmount = invoice.RoomRent + invoice.ServiceFee + invoice.InternetFee + 
                                  invoice.GarbageFee + invoice.OtherFees - invoice.Discount;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
