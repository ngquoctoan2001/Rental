using MediatR;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Properties.Commands.UpdateProperty;

public record UpdatePropertyCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string? Province { get; init; }
    public string? District { get; init; }
    public string? Ward { get; init; }
    public string? Description { get; init; }
    public int TotalFloors { get; init; }
}
