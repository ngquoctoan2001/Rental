using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Customers.Dtos;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Customers.Queries.GetCustomers;

public record GetCustomersQuery : IRequest<PagedResult<CustomerListItemDto>>
{
    public string? Search { get; init; }
    public bool? IsBlacklisted { get; init; }
    public bool? HasActiveContract { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedResult<CustomerListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CustomerListItemDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(c => c.FullName.Contains(request.Search) || 
                                     c.Phone.Contains(request.Search) || 
                                     (c.IdCardNumber != null && c.IdCardNumber.Contains(request.Search)));
        }

        if (request.IsBlacklisted.HasValue)
        {
            query = query.Where(c => c.IsBlacklisted == request.IsBlacklisted.Value);
        }

        if (request.HasActiveContract.HasValue)
        {
            if (request.HasActiveContract.Value)
            {
                query = query.Where(c => c.Contracts.Any(cn => cn.Status == ContractStatus.Active));
            }
            else
            {
                query = query.Where(c => !c.Contracts.Any(cn => cn.Status == ContractStatus.Active));
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CustomerListItemDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Phone = c.Phone,
                IdCardNumber = c.IdCardNumber,
                IsBlacklisted = c.IsBlacklisted,
                CreatedAt = c.CreatedAt,
                ActiveContract = c.Contracts
                    .Where(cn => cn.Status == ContractStatus.Active)
                    .Select(cn => new ActiveContractDto
                    {
                        ContractCode = cn.ContractCode,
                        RoomNumber = cn.Room.RoomNumber,
                        PropertyName = cn.Room.Property.Name
                    })
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerListItemDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
