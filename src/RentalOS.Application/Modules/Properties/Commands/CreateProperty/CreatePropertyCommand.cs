using MediatR;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Properties.Commands.CreateProperty;

public record CreatePropertyCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string? Province { get; init; }
    public string? District { get; init; }
    public string? Ward { get; init; }
    public string? Description { get; init; }
    public int TotalFloors { get; init; } = 1;
}
