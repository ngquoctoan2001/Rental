using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Properties.Commands.UpdateProperty;

public record UpdatePropertyCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string? Province { get; init; }
    public string? District { get; init; }
    public string? Ward { get; init; }
    public string? Description { get; init; }
    public int TotalFloors { get; init; }
    public bool IsActive { get; init; }
}

public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdatePropertyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new Exception("PROPERTY_NOT_FOUND");
        }

        entity.Name = request.Name;
        entity.Address = request.Address;
        entity.Province = request.Province;
        entity.District = request.District;
        entity.Ward = request.Ward;
        entity.Description = request.Description;
        entity.TotalFloors = request.TotalFloors;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
