using MediatR;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Properties.Commands.DeleteProperty;

public record DeletePropertyCommand(Guid Id) : IRequest<Result<bool>>;
