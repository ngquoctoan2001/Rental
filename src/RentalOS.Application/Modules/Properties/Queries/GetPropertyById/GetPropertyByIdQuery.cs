using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.Properties.Dtos;
using RentalOS.Domain.Enums;
using System.Text.Json;

namespace RentalOS.Application.Modules.Properties.Queries.GetPropertyById;

public record GetPropertyByIdQuery(Guid Id) : IRequest<PropertyDto>;

public class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyDto>
{
    private readonly IApplicationDbContext _context;

    public GetPropertyByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyDto> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await _context.Properties
            .Include(p => p.Rooms)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (p == null)
        {
            throw new Exception("PROPERTY_NOT_FOUND");
        }

        return new PropertyDto
        {
            Id = p.Id,
            Name = p.Name,
            Address = p.Address,
            Province = p.Province,
            District = p.District,
            Ward = p.Ward,
            Description = p.Description,
            CoverImage = p.CoverImage,
            Images = string.IsNullOrEmpty(p.Images) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(p.Images)!,
            TotalFloors = p.TotalFloors,
            IsActive = p.IsActive,
            RoomSummary = new RoomSummaryDto
            {
                Total = p.Rooms.Count,
                Available = p.Rooms.Count(r => r.Status == RoomStatus.Available),
                Rented = p.Rooms.Count(r => r.Status == RoomStatus.Rented),
                Maintenance = p.Rooms.Count(r => r.Status == RoomStatus.Maintenance)
            }
        };
    }
}
