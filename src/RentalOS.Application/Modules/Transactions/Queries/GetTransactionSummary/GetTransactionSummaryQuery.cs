using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Transactions.DTOs;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Transactions.Queries.GetTransactionSummary;

public record GetTransactionSummaryQuery : IRequest<Result<TransactionSummaryDto>>
{
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public Guid? PropertyId { get; init; }
}

public class GetTransactionSummaryQueryHandler(IApplicationDbContext context) : IRequestHandler<GetTransactionSummaryQuery, Result<TransactionSummaryDto>>
{
    public async Task<Result<TransactionSummaryDto>> Handle(GetTransactionSummaryQuery request, CancellationToken cancellationToken)
    {
        var transactionsQuery = context.Transactions.AsNoTracking();

        if (request.DateFrom.HasValue)
            transactionsQuery = transactionsQuery.Where(t => t.PaidAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            transactionsQuery = transactionsQuery.Where(t => t.PaidAt <= request.DateTo.Value);
        if (request.PropertyId.HasValue)
            transactionsQuery = transactionsQuery.Where(t => t.Invoice != null && t.Invoice.Contract.Room.PropertyId == request.PropertyId.Value);

        var transactions = await transactionsQuery.ToListAsync(cancellationToken);

        var summary = new TransactionSummaryDto
        {
            TotalIncome = transactions.Where(t => t.Direction == TransactionDirection.Income && t.Status == TransactionStatus.Success).Sum(t => t.Amount),
            TotalExpense = transactions.Where(t => t.Direction == TransactionDirection.Expense && t.Status == TransactionStatus.Success).Sum(t => t.Amount),
        };

        // By Method
        summary.ByMethod = transactions
            .GroupBy(t => t.Method.ToString().ToLower())
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        // By Category
        summary.ByCategory = transactions
            .GroupBy(t => t.Category.ToString().ToLower())
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        // Collection Rate stats
        var invoiceQuery = context.Invoices.AsNoTracking();
        if (request.DateFrom.HasValue)
            invoiceQuery = invoiceQuery.Where(i => i.CreatedAt >= request.DateFrom.Value); // Using CreatedAt for invoices in period
        if (request.DateTo.HasValue)
            invoiceQuery = invoiceQuery.Where(i => i.CreatedAt <= request.DateTo.Value);
        if (request.PropertyId.HasValue)
            invoiceQuery = invoiceQuery.Where(i => i.Contract.Room.PropertyId == request.PropertyId.Value);

        var invoices = await invoiceQuery.ToListAsync(cancellationToken);

        summary.TotalInvoiced = invoices.Sum(i => i.TotalAmount);
        summary.TotalCollected = summary.TotalIncome; // Summary from transactions
        summary.TotalOutstanding = summary.TotalInvoiced - summary.TotalCollected;
        summary.CollectionRate = summary.TotalInvoiced > 0 ? (summary.TotalCollected / summary.TotalInvoiced * 100) : 0;

        return Result<TransactionSummaryDto>.Ok(summary);
    }
}
