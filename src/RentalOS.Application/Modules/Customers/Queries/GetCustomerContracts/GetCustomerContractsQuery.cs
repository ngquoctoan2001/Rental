using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Customers.Queries.GetCustomerContracts;

public record GetCustomerContractsQuery(Guid CustomerId) : IRequest<Result<List<CustomerContractDto>>>;

public record CustomerContractDto
{
    public Guid Id { get; init; }
    public string ContractCode { get; init; } = string.Empty;
    public string RoomNumber { get; init; } = string.Empty;
    public string PropertyName { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal MonthlyRent { get; init; }
    public string Status { get; init; } = string.Empty;
}

public class GetCustomerContractsQueryHandler : IRequestHandler<GetCustomerContractsQuery, Result<List<CustomerContractDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerContractsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CustomerContractDto>>> Handle(GetCustomerContractsQuery request, CancellationToken cancellationToken)
    {
        var contracts = await _context.Contracts
            .Where(c => c.CustomerId == request.CustomerId || c.CoTenants.Any(ct => ct.CustomerId == request.CustomerId))
            .OrderByDescending(c => c.StartDate)
            .Select(c => new CustomerContractDto
            {
                Id = c.Id,
                ContractCode = c.ContractCode,
                RoomNumber = c.Room.RoomNumber,
                PropertyName = c.Room.Property.Name,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                MonthlyRent = c.MonthlyRent,
                Status = c.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return Result<List<CustomerContractDto>>.Ok(contracts);
    }
}
