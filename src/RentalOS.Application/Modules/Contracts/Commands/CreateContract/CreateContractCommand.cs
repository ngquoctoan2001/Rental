using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;
using RentalOS.Domain.Exceptions;
using RentalOS.Application.Common.BackgroundJobs;

namespace RentalOS.Application.Modules.Contracts.Commands.CreateContract;

public record CreateContractCommand : IRequest<Result<Guid>>
{
    public Guid RoomId { get; init; }
    public Guid CustomerId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal MonthlyRent { get; init; }
    public int DepositMonths { get; init; } = 1;
    public decimal? ElectricityPrice { get; init; }
    public decimal? WaterPrice { get; init; }
    public decimal? ServiceFee { get; init; }
    public decimal? InternetFee { get; init; }
    public decimal? GarbageFee { get; init; }
    public int BillingDate { get; init; } = 5;
    public int PaymentDueDays { get; init; } = 10;
    public int MaxOccupants { get; init; } = 2;
    public string? Terms { get; init; }
}

public class CreateContractCommandHandler(
    IApplicationDbContext context, 
    IBackgroundJobService backgroundJobService,
    ICurrentUserService currentUserService) : IRequestHandler<CreateContractCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateContractCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Validate room status == available
            // 4. SELECT FOR UPDATE on rooms row (combined)
            var room = await context.Rooms
                .FromSqlRaw("SELECT * FROM rooms WHERE \"Id\" = {0} FOR UPDATE", request.RoomId)
                .FirstOrDefaultAsync(cancellationToken);

            if (room == null) return Result<Guid>.Fail("ROOM_NOT_FOUND", "Không tìm thấy phòng.");
            if (room.Status != RoomStatus.Available) 
                throw new RoomNotAvailableException(room.Id, room.Status.ToString());

            // 2. Validate customer is_blacklisted == false
            var customer = await context.Customers
                .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
            
            if (customer == null) return Result<Guid>.Fail("CUSTOMER_NOT_FOUND", "Không tìm thấy khách hàng.");
            if (customer.IsBlacklisted) 
                throw new CustomerBlacklistedException(customer.Id, customer.BlacklistReason);

            // 3. Validate no active contract for this room
            var hasActiveContract = await context.Contracts
                .AnyAsync(c => c.RoomId == request.RoomId && c.Status == ContractStatus.Active, cancellationToken);
            if (hasActiveContract) return Result<Guid>.Fail("CONTRACT_EXISTS", "Phòng đã có hợp đồng còn hiệu lực.");

            // 5. Generate contract_code: HD-{YYYY}-{sequence+1 padLeft 3}
            var year = request.StartDate.Year;
            var sequence = await context.Contracts
                .CountAsync(c => c.CreatedAt.Year == DateTime.UtcNow.Year, cancellationToken);
            var contractCode = $"HD-{year}-{(sequence + 1).ToString().PadLeft(3, '0')}";

            // 6. Set deposit_amount = monthly_rent * deposit_months
            var depositAmount = request.MonthlyRent * request.DepositMonths;

            // 7. INSERT contract
            var contract = new Contract
            {
                Id = Guid.NewGuid(),
                RoomId = request.RoomId,
                CustomerId = request.CustomerId,
                ContractCode = contractCode,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                MonthlyRent = request.MonthlyRent,
                DepositMonths = request.DepositMonths,
                DepositAmount = depositAmount,
                ElectricityPrice = request.ElectricityPrice ?? room.ElectricityPrice,
                WaterPrice = request.WaterPrice ?? room.WaterPrice,
                ServiceFee = request.ServiceFee ?? room.ServiceFee,
                InternetFee = request.InternetFee ?? room.InternetFee,
                GarbageFee = request.GarbageFee ?? room.GarbageFee,
                BillingDate = request.BillingDate,
                PaymentDueDays = request.PaymentDueDays,
                MaxOccupants = request.MaxOccupants,
                Terms = request.Terms,
                Status = ContractStatus.Active,
                CreatedBy = Guid.TryParse(currentUserService.UserId, out var creatorId) ? creatorId : null
            };

            // 8. UPDATE rooms.status = 'rented'
            room.Status = RoomStatus.Rented;

            // 9. INSERT contract_co_tenants (primary customer)
            var coTenant = new ContractCoTenant
            {
                ContractId = contract.Id,
                CustomerId = request.CustomerId,
                IsPrimary = true,
                MovedInAt = request.StartDate
            };

            context.Contracts.Add(contract);
            context.ContractCoTenants.Add(coTenant);

            // 11. Ghi AuditLog
            var auditLog = new AuditLog
            {
                Action = "contracts.create",
                EntityType = "Contract",
                EntityId = contract.Id,
                EntityCode = contract.ContractCode,
                UserId = Guid.TryParse(currentUserService.UserId, out var auditorId) ? auditorId : null,
                NewValue = JsonSerializer.Serialize(contract)
            };
            context.AuditLogs.Add(auditLog);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // 10. Enqueue Hangfire job: GenerateContractPdfJob(contractId)
            backgroundJobService.Enqueue<GenerateContractPdfJob>(j => j.Execute(contract.Id));

            return Result<Guid>.Ok(contract.Id);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
