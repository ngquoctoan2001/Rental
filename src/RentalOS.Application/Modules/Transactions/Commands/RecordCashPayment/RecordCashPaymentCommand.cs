using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;
using System.Text.Json;

namespace RentalOS.Application.Modules.Transactions.Commands.RecordCashPayment;

public record RecordCashPaymentCommand(Guid InvoiceId, decimal Amount, DateTime PaidAt, string? Note) : IRequest<Result<Guid>>;

public class RecordCashPaymentCommandHandler(IApplicationDbContext context) : IRequestHandler<RecordCashPaymentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RecordCashPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null) return Result<Guid>.Fail("NOT_FOUND", "Không tìm thấy hóa đơn.");
        
        if (invoice.Status == InvoiceStatus.Paid) 
            return Result<Guid>.Fail("ALREADY_PAID", "Hóa đơn này đã được thanh toán.");

        // Create transaction
        var transaction = new Transaction
        {
            InvoiceId = invoice.Id,
            Amount = request.Amount,
            Method = TransactionMethod.Cash,
            Direction = TransactionDirection.Income,
            Category = TransactionCategory.Rent,
            Status = TransactionStatus.Success,
            Note = request.Note,
            PaidAt = request.PaidAt,
            RecordedBy = null // TODO: Get from current user context
        };

        context.Transactions.Add(transaction);

        // Update invoice
        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAt = request.PaidAt;
        invoice.PartialPaidAmount = (invoice.PartialPaidAmount ?? 0) + request.Amount;

        // Audit Log
        context.AuditLogs.Add(new AuditLog
        {
            Action = "invoice.paid_cash",
            EntityType = "Invoice",
            EntityId = invoice.Id,
            EntityCode = invoice.InvoiceCode,
            NewValue = JsonSerializer.Serialize(new
            {
                invoiceId = invoice.Id,
                amount = request.Amount,
                paidAt = request.PaidAt,
                method = TransactionMethod.Cash.ToString()
            }),
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(transaction.Id);
    }
}
