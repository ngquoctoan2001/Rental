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
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
