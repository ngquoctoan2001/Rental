using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Transactions.Commands.RecordDepositRefund;

public record RecordDepositRefundCommand(Guid ContractId, decimal Amount, TransactionMethod Method, DateTime PaidAt, string? Note) : IRequest<Result<Guid>>;

public class RecordDepositRefundCommandHandler(IApplicationDbContext context) : IRequestHandler<RecordDepositRefundCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RecordDepositRefundCommand request, CancellationToken cancellationToken)
    {
        var contract = await context.Contracts
            .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

        if (contract == null) return Result<Guid>.Fail("NOT_FOUND", "Không tìm thấy hợp đồng.");

        // Create transaction
        var transaction = new Transaction
        {
            Amount = request.Amount,
            Method = request.Method,
            Direction = TransactionDirection.Expense,
            Category = TransactionCategory.DepositRefund,
            Status = TransactionStatus.Success,
            Note = request.Note,
            PaidAt = request.PaidAt,
            RecordedBy = null // TODO: Get from current user context
        };

        context.Transactions.Add(transaction);

        // Update contract
        contract.DepositRefunded = (contract.DepositRefunded ?? 0) + request.Amount;

        // Audit Log
        context.AuditLogs.Add(new AuditLog
        {
            Action = "contract.deposit_refunded",
            EntityType = "Contract",
            EntityId = contract.Id,
            EntityCode = contract.ContractCode,
            NewValue = $"Refunded amount: {request.Amount}",
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(transaction.Id);
    }
}
