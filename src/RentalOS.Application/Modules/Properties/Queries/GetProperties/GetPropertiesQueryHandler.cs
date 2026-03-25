using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Properties.Dtos;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Properties.Queries.GetProperties;

public class GetPropertiesQueryHandler(IApplicationDbContext context) 
    : IRequestHandler<GetPropertiesQuery, PagedResult<PropertyListItemDto>>
{
    public async Task<PagedResult<PropertyListItemDto>> Handle(GetPropertiesQuery request, CancellationToken cancellationToken)
    {
        var query = context.Properties
            .Include(p => p.Rooms)
            .AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p => p.Name.Contains(request.Search) || p.Address.Contains(request.Search));
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
