using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.MeterReadings.Dtos;

namespace RentalOS.Application.Modules.Rooms.Queries.GetRoomMeterReadings;

public record GetRoomMeterReadingsQuery(Guid Id, string? Month) : IRequest<List<MeterReadingDto>>;

public class GetRoomMeterReadingsQueryHandler : IRequestHandler<GetRoomMeterReadingsQuery, List<MeterReadingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRoomMeterReadingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MeterReadingDto>> Handle(GetRoomMeterReadingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MeterReadings
            .Include(m => m.Room)
            .Where(m => m.RoomId == request.Id)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(request.Month))
        {
            // Month format: YYYY-MM
            if (DateTime.TryParse(request.Month + "-01", out var date))
            {
                var startOfMonth = new DateTime(date.Year, date.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1);
                query = query.Where(m => m.ReadingDate >= startOfMonth && m.ReadingDate < endOfMonth);
            }
        }

        return await query
            .OrderByDescending(m => m.ReadingDate)
            .Select(m => new MeterReadingDto
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
            })
            .ToListAsync(cancellationToken);
    }
}
