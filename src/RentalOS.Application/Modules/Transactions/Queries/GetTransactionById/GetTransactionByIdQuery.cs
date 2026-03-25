using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Transactions.DTOs;

namespace RentalOS.Application.Modules.Transactions.Queries.GetTransactionById;

public record GetTransactionByIdQuery(Guid Id) : IRequest<Result<TransactionDto>>;

public class GetTransactionByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetTransactionByIdQuery, Result<TransactionDto>>
{
    public async Task<Result<TransactionDto>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await context.Transactions
            .Include(t => t.Invoice)
                .ThenInclude(i => i!.Contract)
                    .ThenInclude(c => c.Room)
                        .ThenInclude(r => r.Property)
            .AsNoTracking()
            .Where(t => t.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (transaction == null) return Result<TransactionDto>.Fail("NOT_FOUND", "Không tìm thấy giao dịch.");

        return Result<TransactionDto>.Ok(transaction);
    }
}
