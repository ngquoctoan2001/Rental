using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Contracts.Queries.GetContractInvoices;

public record GetContractInvoicesQuery(Guid ContractId) : IRequest<Result<List<ContractInvoiceDto>>>;

public record ContractInvoiceDto(
    Guid Id,
    string InvoiceCode,
    decimal TotalAmount,
    DateOnly DueDate,
    string Status);

public class GetContractInvoicesQueryHandler(IApplicationDbContext context) : IRequestHandler<GetContractInvoicesQuery, Result<List<ContractInvoiceDto>>>
{
    public async Task<Result<List<ContractInvoiceDto>>> Handle(GetContractInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await context.Invoices
            .Where(i => i.ContractId == request.ContractId)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new ContractInvoiceDto(
                i.Id,
                i.InvoiceCode,
                i.TotalAmount,
                i.DueDate,
                i.Status.ToString()))
            .ToListAsync(cancellationToken);

        return Result<List<ContractInvoiceDto>>.Ok(invoices);
    }
}
