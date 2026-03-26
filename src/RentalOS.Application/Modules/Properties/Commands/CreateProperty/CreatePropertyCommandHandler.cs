using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Properties.Commands.CreateProperty;

public class CreatePropertyCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext) : IRequestHandler<CreatePropertyCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra plan limit
        var currentCount = await context.Properties.CountAsync(p => p.IsActive, cancellationToken);
        
        bool canCreate = tenantContext.Plan switch
        {
            PlanType.Trial => currentCount < 1,
            PlanType.Starter => currentCount < 1,
            PlanType.Pro => currentCount < 3,
            PlanType.Business => true,
            _ => false
        };

        if (!canCreate)
        {
            return Result<Guid>.Fail("TENANT_PLAN_LIMIT", "Bạn đã đạt giới hạn nhà trọ của gói hiện tại.");
        }

        var property = new Property
        {
            Name = request.Name,
            Address = request.Address,
            Province = request.Province,
            District = request.District,
            Ward = request.Ward,
            Description = request.Description,
            TotalFloors = request.TotalFloors,
            OwnerId = tenantContext.UserId, // Gán cho user hiện tại (owner/manager tạo)
            IsActive = true
        };

        context.Properties.Add(property);
        await context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(property.Id);
    }
}
