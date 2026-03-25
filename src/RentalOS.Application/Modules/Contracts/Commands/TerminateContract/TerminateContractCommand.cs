using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Contracts.Commands.TerminateContract;

public record TerminateContractCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public DateTime TerminatedAt { get; init; } = DateTime.UtcNow;
    public string? Reason { get; init; }
    public TerminationType Type { get; init; } = TerminationType.Normal;
    public decimal? DepositRefunded { get; init; }
}

public class TerminateContractCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<TerminateContractCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(TerminateContractCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var contract = await context.Contracts
                .Include(c => c.Room)
                .Include(c => c.CoTenants)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            
            if (contract == null) return Result<bool>.Fail("CONTRACT_NOT_FOUND", "Không tìm thấy hợp đồng.");
            if (contract.Status == ContractStatus.Terminated) return Result<bool>.Fail("ALREADY_TERMINATED", "Hợp đồng đã thanh lý trước đó.");

            var oldContractValue = JsonSerializer.Serialize(contract);

            contract.Status = ContractStatus.Terminated;
            contract.TerminatedAt = request.TerminatedAt;
            contract.TerminationReason = request.Reason;
            contract.TerminationType = request.Type;
            contract.DepositRefunded = request.DepositRefunded;

            // Reset room status
            contract.Room.Status = RoomStatus.Available;

            // Set moved out date for all co-tenants
            var outDate = DateOnly.FromDateTime(request.TerminatedAt);
            foreach (var coTenant in contract.CoTenants)
            {
                if (coTenant.MovedOutAt == null)
                {
                    coTenant.MovedOutAt = outDate;
                }
            }

            // Ghi AuditLog
            var auditLog = new AuditLog
            {
                Action = "contracts.terminate",
                EntityType = "Contract",
                EntityId = contract.Id,
                EntityCode = contract.ContractCode,
                UserId = Guid.TryParse(currentUserService.UserId, out var auditorId) ? auditorId : null,
                OldValue = oldContractValue,
                NewValue = JsonSerializer.Serialize(contract)
            };
            context.AuditLogs.Add(auditLog);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<bool>.Ok(true);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
