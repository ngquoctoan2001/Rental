using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Properties.Dtos;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Properties.Queries.GetProperties;

public record GetPropertiesQuery : IRequest<PagedResult<PropertyListItemDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public bool? IsActive { get; init; }
}

public class GetPropertiesQueryHandler : IRequestHandler<GetPropertiesQuery, PagedResult<PropertyListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPropertiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PropertyListItemDto>> Handle(GetPropertiesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Properties
            .Include(p => p.Rooms)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(p => p.Name.Contains(request.Search) || p.Address.Contains(request.Search));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PropertyListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                CoverImage = p.CoverImage,
                IsActive = p.IsActive,
                RoomSummary = new RoomSummaryDto
                {
                    Total = p.Rooms.Count,
                    Available = p.Rooms.Count(r => r.Status == RoomStatus.Available),
                    Rented = p.Rooms.Count(r => r.Status == RoomStatus.Rented),
                    Maintenance = p.Rooms.Count(r => r.Status == RoomStatus.Maintenance)
                }
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PropertyListItemDto>(items, totalCount, request.Page, request.PageSize);
    }
}
