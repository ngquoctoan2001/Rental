using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Rooms.Dtos;
using RentalOS.Domain.Enums;
using System.Text.Json;

namespace RentalOS.Application.Modules.Rooms.Queries.GetRooms;

public record GetRoomsQuery : IRequest<PagedResult<RoomListItemDto>>
{
    public Guid? PropertyId { get; init; }
    public RoomStatus? Status { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? Floor { get; init; }
    public string? Amenities { get; init; } // Comma separated
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, PagedResult<RoomListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRoomsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<RoomListItemDto>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Rooms
            .Include(r => r.Property)
            .Where(r => r.IsActive)
            .AsNoTracking();

        if (request.PropertyId.HasValue)
            query = query.Where(r => r.PropertyId == request.PropertyId.Value);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        if (request.MinPrice.HasValue)
            query = query.Where(r => r.BasePrice >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(r => r.BasePrice <= request.MaxPrice.Value);

        if (request.Floor.HasValue)
            query = query.Where(r => r.Floor == request.Floor.Value);

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(r => r.RoomNumber.Contains(request.Search) || (r.Notes != null && r.Notes.Contains(request.Search)));
        }

        if (!string.IsNullOrEmpty(request.Amenities))
        {
            var requestedAmenities = request.Amenities.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim().ToLower());
            foreach (var amenity in requestedAmenities)
            {
                // This is a bit slow on large datasets because of JSON string search, 
                // but for managed rental it's typically fine.
                query = query.Where(r => r.Amenities.Contains(amenity));
            }
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(r => r.Floor)
            .ThenBy(r => r.RoomNumber)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new RoomListItemDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Floor = r.Floor,
                BasePrice = r.BasePrice,
                Status = r.Status,
                PropertyName = r.Property.Name
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<RoomListItemDto>(items, total, request.Page, request.PageSize);
    }
}
