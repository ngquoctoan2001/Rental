using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Properties.Commands.CreateProperty;

public record CreatePropertyCommand : IRequest<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string? Province { get; init; }
    public string? District { get; init; }
    public string? Ward { get; init; }
    public string? Description { get; init; }
    public int TotalFloors { get; init; } = 1;
}

public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Tên nhà trọ không được để trống.")
            .MinimumLength(2).WithMessage("Tên nhà trọ phải từ 2 ký tự trở lên.")
            .MaximumLength(200).WithMessage("Tên nhà trọ không được quá 200 ký tự.");

        RuleFor(v => v.Address)
            .NotEmpty().WithMessage("Địa chỉ không được để trống.");
    }
}

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreatePropertyCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Guid> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        // 1. Check plan limit
        var propertyCount = await _context.Properties.CountAsync(cancellationToken);
        
        var maxProperties = _tenantContext.PlanType switch
        {
            PlanType.Starter => 1,
            PlanType.Pro => 3,
            PlanType.Business => int.MaxValue,
            PlanType.Trial => 1, // Default trial limit
            _ => 1
        };

        if (propertyCount >= maxProperties)
        {
            throw new Exception("TENANT_PLAN_LIMIT: Bạn đã đạt giới hạn tối đa số lượng nhà trọ cho gói dịch vụ hiện tại.");
        }

        // 2. Create property
        var property = new Property
        {
            Name = request.Name,
            Address = request.Address,
            Province = request.Province,
            District = request.District,
            Ward = request.Ward,
            Description = request.Description,
            TotalFloors = request.TotalFloors,
            OwnerId = _tenantContext.UserId,
            IsActive = true
        };

        _context.Properties.Add(property);
        await _context.SaveChangesAsync(cancellationToken);

        return property.Id;
    }
}
