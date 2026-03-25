using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Customers.Queries.SearchCustomers;

public record SearchCustomersQuery(string Query) : IRequest<List<CustomerLookupDto>>;

public record CustomerLookupDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? IdCardNumber { get; init; }
}

public class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, List<CustomerLookupDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerLookupDto>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query)) return [];

        return await _context.Customers
            .Where(c => c.FullName.Contains(request.Query) || 
                         c.Phone.Contains(request.Query) || 
                         (c.IdCardNumber != null && c.IdCardNumber.Contains(request.Query)))
            .Take(10)
            .Select(c => new CustomerLookupDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Phone = c.Phone,
                IdCardNumber = c.IdCardNumber
            })
            .ToListAsync(cancellationToken);
    }
}
