using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.CoTenants.Queries.GetCoTenants;

public record GetCoTenantsQuery(Guid ContractId) : IRequest<Result<List<CoTenantDto>>>;

public record CoTenantDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? IdCardNumber { get; init; }
    public bool IsPrimary { get; init; }
    public DateOnly? MovedInAt { get; init; }
    public DateOnly? MovedOutAt { get; init; }
}

public class GetCoTenantsQueryHandler : IRequestHandler<GetCoTenantsQuery, Result<List<CoTenantDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCoTenantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CoTenantDto>>> Handle(GetCoTenantsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.ContractCoTenants
            .Where(ct => ct.ContractId == request.ContractId)
            .OrderByDescending(ct => ct.IsPrimary)
            .ThenBy(ct => ct.MovedInAt)
            .Select(ct => new CoTenantDto
            {
                Id = ct.Id,
                CustomerId = ct.CustomerId,
                FullName = ct.Customer.FullName,
                Phone = ct.Customer.Phone,
                IdCardNumber = ct.Customer.IdCardNumber,
                IsPrimary = ct.IsPrimary,
                MovedInAt = ct.MovedInAt,
                MovedOutAt = ct.MovedOutAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<CoTenantDto>>.Ok(items);
    }
}
