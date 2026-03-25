using MediatR;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Properties.Dtos;

namespace RentalOS.Application.Modules.Properties.Queries.GetPropertyById;

public record GetPropertyByIdQuery(Guid Id) : IRequest<Result<PropertyDto>>;
