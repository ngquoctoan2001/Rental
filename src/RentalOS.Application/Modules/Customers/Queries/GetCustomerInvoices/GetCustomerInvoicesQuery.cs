using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Customers.Queries.GetCustomerInvoices;

public record GetCustomerInvoicesQuery(Guid CustomerId) : IRequest<Result<List<CustomerInvoiceDto>>>;

public record CustomerInvoiceDto
{
    public Guid Id { get; init; }
    public string InvoiceCode { get; init; } = string.Empty;
    public string BillingMonth { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? PaidAt { get; init; }
    public DateTime DueDate { get; init; }
}

public class GetCustomerInvoicesQueryHandler : IRequestHandler<GetCustomerInvoicesQuery, Result<List<CustomerInvoiceDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerInvoicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CustomerInvoiceDto>>> Handle(GetCustomerInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _context.Invoices
            .Where(i => i.Contract.CustomerId == request.CustomerId)
            .OrderByDescending(i => i.BillingMonth)
            .Select(i => new CustomerInvoiceDto
            {
                Id = i.Id,
                InvoiceCode = i.InvoiceCode,
                BillingMonth = i.BillingMonth.ToString("MM/yyyy"),
                TotalAmount = i.TotalAmount,
                Status = i.Status.ToString(),
                PaidAt = i.PaidAt,
                DueDate = i.DueDate.ToDateTime(TimeOnly.MinValue)
            })
            .ToListAsync(cancellationToken);

        return Result<List<CustomerInvoiceDto>>.Ok(invoices);
    }
}
