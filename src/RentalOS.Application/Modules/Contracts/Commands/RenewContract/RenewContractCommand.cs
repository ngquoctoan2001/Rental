using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;
using RentalOS.Application.Common.BackgroundJobs;

namespace RentalOS.Application.Modules.Contracts.Commands.RenewContract;

public record RenewContractCommand : IRequest<Result<Guid>>
{
    public Guid OldContractId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal? NewMonthlyRent { get; init; }
}

public class RenewContractCommandHandler(
    IApplicationDbContext context,
    IBackgroundJobService backgroundJobService,
    ICurrentUserService currentUserService) : IRequestHandler<RenewContractCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RenewContractCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var oldContract = await context.Contracts
                .Include(c => c.CoTenants)
                .FirstOrDefaultAsync(c => c.Id == request.OldContractId, cancellationToken);

            if (oldContract == null) return Result<Guid>.Fail("CONTRACT_NOT_FOUND", "Không tìm thấy hợp đồng cũ.");
            if (oldContract.Status != ContractStatus.Active) 
                return Result<Guid>.Fail("INVALID_STATUS", "Chỉ có thể gia hạn hợp đồng đang còn hiệu lực.");

            // 1. Mark old contract as Renewed
            oldContract.Status = ContractStatus.Renewed;

            // 2. Generate new contract code
            var year = request.StartDate.Year;
            var sequence = await context.Contracts
                .CountAsync(c => c.CreatedAt.Year == DateTime.UtcNow.Year, cancellationToken);
            var contractCode = $"HD-{year}-{(sequence + 1).ToString().PadLeft(3, '0')}";

            // 3. Create new contract
            var newContract = new Contract
            {
                Id = Guid.NewGuid(),
                RoomId = oldContract.RoomId,
                CustomerId = oldContract.CustomerId,
                ContractCode = contractCode,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                MonthlyRent = request.NewMonthlyRent ?? oldContract.MonthlyRent,
                DepositMonths = oldContract.DepositMonths,
                DepositAmount = oldContract.DepositAmount, // Usually carry over deposit
                ElectricityPrice = oldContract.ElectricityPrice,
                WaterPrice = oldContract.WaterPrice,
                ServiceFee = oldContract.ServiceFee,
                InternetFee = oldContract.InternetFee,
                GarbageFee = oldContract.GarbageFee,
                BillingDate = oldContract.BillingDate,
                PaymentDueDays = oldContract.PaymentDueDays,
                MaxOccupants = oldContract.MaxOccupants,
                Terms = oldContract.Terms,
                Status = ContractStatus.Active,
                CreatedBy = Guid.TryParse(currentUserService.UserId, out var creatorId) ? creatorId : null
            };

            context.Contracts.Add(newContract);

            // 4. Carry over co-tenants
            foreach (var ct in oldContract.CoTenants.Where(t => t.MovedOutAt == null))
            {
                context.ContractCoTenants.Add(new ContractCoTenant
                {
                    ContractId = newContract.Id,
                    CustomerId = ct.CustomerId,
                    IsPrimary = ct.IsPrimary,
                    MovedInAt = request.StartDate
                });
            }

            // 5. Audit logs
            context.AuditLogs.Add(new AuditLog
            {
                Action = "contracts.renew",
                EntityType = "Contract",
                EntityId = oldContract.Id,
                EntityCode = oldContract.ContractCode,
                UserId = Guid.TryParse(currentUserService.UserId, out var auditorId) ? auditorId : null,
                NewValue = $"Renewed by {newContract.ContractCode}"
            });

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // 6. Enqueue PDF job for new contract
#pragma warning disable CS4014 // Hangfire serializes the expression, not awaiting by design
            backgroundJobService.Enqueue<GenerateContractPdfJob>(j => j.Execute(newContract.Id));
#pragma warning restore CS4014

            return Result<Guid>.Ok(newContract.Id);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
