using MediatR;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Properties.Dtos;

namespace RentalOS.Application.Modules.Properties.Queries.GetPropertyStats;

public record GetPropertyStatsQuery(Guid Id) : IRequest<Result<PropertyStatsDto>>;
