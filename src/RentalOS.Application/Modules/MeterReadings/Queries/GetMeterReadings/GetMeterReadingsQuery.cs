using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.MeterReadings.Dtos;

namespace RentalOS.Application.Modules.MeterReadings.Queries.GetMeterReadings;

public record GetMeterReadingsQuery : IRequest<PagedResult<MeterReadingDto>>
{
    public Guid? RoomId { get; init; }
    public Guid? PropertyId { get; init; }
    public string? Month { get; init; } // YYYY-MM
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetMeterReadingsQueryHandler : IRequestHandler<GetMeterReadingsQuery, PagedResult<MeterReadingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMeterReadingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<MeterReadingDto>> Handle(GetMeterReadingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MeterReadings
            .Include(m => m.Room)
            .ThenInclude(r => r.Property)
            .AsNoTracking();

        if (request.RoomId.HasValue)
            query = query.Where(m => m.RoomId == request.RoomId.Value);

        if (request.PropertyId.HasValue)
            query = query.Where(m => m.Room.PropertyId == request.PropertyId.Value);

        if (!string.IsNullOrEmpty(request.Month))
        {
            if (DateTime.TryParse(request.Month + "-01", out var date))
            {
                var startOfMonth = new DateOnly(date.Year, date.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1);
                query = query.Where(m => m.ReadingDate >= startOfMonth && m.ReadingDate < endOfMonth);
            }
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(m => m.ReadingDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MeterReadingDto
            {
                Id = m.Id,
                RoomId = m.RoomId,
                RoomNumber = m.Room.RoomNumber,
                PropertyName = m.Room.Property.Name,
                ReadingDate = m.ReadingDate,
                ElectricityReading = m.ElectricityReading,
                WaterReading = m.WaterReading,
                ElectricityImage = m.ElectricityImage,
                WaterImage = m.WaterImage,
                Note = m.Note
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<MeterReadingDto>(items, total, request.Page, request.PageSize);
    }
}
