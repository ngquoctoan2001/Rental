using MediatR;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Properties.Dtos;

namespace RentalOS.Application.Modules.Properties.Queries.GetProperties;

public record GetPropertiesQuery : IRequest<PagedResult<PropertyListItemDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public bool? IsActive { get; init; } = true;
}
