using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Properties.Dtos;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Properties.Queries.GetPropertyById;

public class GetPropertyByIdQueryHandler(IApplicationDbContext context) 
    : IRequestHandler<GetPropertyByIdQuery, Result<PropertyDto>>
{
    public async Task<Result<PropertyDto>> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await context.Properties
            .Include(p => p.Rooms)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (property == null)
        {
            return Result<PropertyDto>.Fail("PROPERTY_NOT_FOUND", "Không tìm thấy nhà trọ.");
        }

        var dto = new PropertyDto
        {
            Id = property.Id,
            Name = property.Name,
            Address = property.Address,
            Province = property.Province,
            District = property.District,
            Ward = property.Ward,
            Description = property.Description,
            CoverImage = property.CoverImage,
            Images = System.Text.Json.JsonSerializer.Deserialize<List<string>>(property.Images) ?? [],
            TotalFloors = property.TotalFloors,
            IsActive = property.IsActive,
            RoomSummary = new RoomSummaryDto
            {
                Total = property.Rooms.Count,
                Available = property.Rooms.Count(r => r.Status == RoomStatus.Available),
                Rented = property.Rooms.Count(r => r.Status == RoomStatus.Rented),
                Maintenance = property.Rooms.Count(r => r.Status == RoomStatus.Maintenance)
            }
        };

        return Result<PropertyDto>.Ok(dto);
    }
}
