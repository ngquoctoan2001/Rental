using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Transactions.DTOs;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Transactions.Queries.GetTransactions;

public record GetTransactionsQuery : IRequest<Result<PagedResult<TransactionDto>>>
{
    public Guid? PropertyId { get; init; }
    public Guid? InvoiceId { get; init; }
    public TransactionMethod? Method { get; init; }
    public TransactionDirection? Direction { get; init; }
    public TransactionCategory? Category { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetTransactionsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetTransactionsQuery, Result<PagedResult<TransactionDto>>>
{
    public async Task<Result<PagedResult<TransactionDto>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Transactions
            .Include(t => t.Invoice)
                .ThenInclude(i => i!.Contract)
                    .ThenInclude(c => c.Room)
                        .ThenInclude(r => r.Property)
            .AsNoTracking();

        // Filters
        if (request.PropertyId.HasValue)
            query = query.Where(t => t.Invoice != null && t.Invoice.Contract.Room.PropertyId == request.PropertyId.Value);

        if (request.InvoiceId.HasValue)
            query = query.Where(t => t.InvoiceId == request.InvoiceId.Value);

        if (request.Method.HasValue)
            query = query.Where(t => t.Method == request.Method.Value);

        if (request.Direction.HasValue)
            query = query.Where(t => t.Direction == request.Direction.Value);

        if (request.Category.HasValue)
            query = query.Where(t => t.Category == request.Category.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(t => t.PaidAt >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(t => t.PaidAt <= request.DateTo.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(t => t.PaidAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                InvoiceId = t.InvoiceId,
                InvoiceCode = t.Invoice != null ? t.Invoice.InvoiceCode : null,
                TransactionCode = t.TransactionCode,
                Amount = t.Amount,
                Method = t.Method,
                Direction = t.Direction,
                Category = t.Category,
                ProviderRef = t.ProviderRef,
                Status = t.Status,
                Note = t.Note,
                PaidAt = t.PaidAt,
                CreatedAt = t.CreatedAt,
                RoomNumber = t.Invoice != null ? t.Invoice.Contract.Room.RoomNumber : null,
                PropertyName = t.Invoice != null ? t.Invoice.Contract.Room.Property.Name : null,
                CustomerName = t.Invoice != null ? t.Invoice.Contract.Customer.FullName : null
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<TransactionDto>>.Ok(new PagedResult<TransactionDto>(items, totalCount, request.Page, request.PageSize));
    }
}
