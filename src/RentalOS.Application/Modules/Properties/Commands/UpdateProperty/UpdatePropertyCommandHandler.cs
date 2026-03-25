using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Properties.Commands.UpdateProperty;

public class UpdatePropertyCommandHandler(IApplicationDbContext context) 
    : IRequestHandler<UpdatePropertyCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (property == null)
        {
            return Result<bool>.Fail("PROPERTY_NOT_FOUND", "Không tìm thấy nhà trọ.");
        }

        property.Name = request.Name;
        property.Address = request.Address;
        property.Province = request.Province;
        property.District = request.District;
        property.Ward = request.Ward;
        property.Description = request.Description;
        property.TotalFloors = request.TotalFloors;

        await context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
