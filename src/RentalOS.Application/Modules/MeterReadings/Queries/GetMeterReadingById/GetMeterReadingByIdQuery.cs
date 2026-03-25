using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.MeterReadings.Dtos;

namespace RentalOS.Application.Modules.MeterReadings.Queries.GetMeterReadingById;

public record GetMeterReadingByIdQuery(Guid Id) : IRequest<MeterReadingDto>;

public class GetMeterReadingByIdQueryHandler : IRequestHandler<GetMeterReadingByIdQuery, MeterReadingDto>
{
    private readonly IApplicationDbContext _context;

    public GetMeterReadingByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MeterReadingDto> Handle(GetMeterReadingByIdQuery request, CancellationToken cancellationToken)
    {
        var m = await _context.MeterReadings
            .Include(m => m.Room)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (m == null)
        {
            throw new Exception("METER_READING_NOT_FOUND");
        }

        return new MeterReadingDto
        {
            Id = m.Id,
            RoomId = m.RoomId,
            RoomNumber = m.Room.RoomNumber,
            ReadingDate = m.ReadingDate,
            ElectricityReading = m.ElectricityReading,
            WaterReading = m.WaterReading,
            ElectricityImage = m.ElectricityImage,
            WaterImage = m.WaterImage,
            Note = m.Note
        };
    }
}
