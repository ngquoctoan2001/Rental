using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Contracts.Commands.UpdateContract;

public record UpdateContractCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal MonthlyRent { get; init; }
    public decimal DepositAmount { get; init; }
    public decimal? ElectricityPrice { get; init; }
    public decimal? WaterPrice { get; init; }
    public decimal? ServiceFee { get; init; }
    public decimal? InternetFee { get; init; }
    public decimal? GarbageFee { get; init; }
    public int BillingDate { get; init; }
    public int PaymentDueDays { get; init; }
    public int MaxOccupants { get; init; }
    public string? Terms { get; init; }
    public ContractStatus Status { get; init; }
}

public class UpdateContractCommandHandler : IRequestHandler<UpdateContractCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateContractCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        
        if (contract == null) return Result<bool>.Fail("CONTRACT_NOT_FOUND", "Không tìm thấy hợp đồng.");

        contract.StartDate = request.StartDate;
        contract.EndDate = request.EndDate;
        contract.MonthlyRent = request.MonthlyRent;
        contract.DepositAmount = request.DepositAmount;
        contract.ElectricityPrice = request.ElectricityPrice;
        contract.WaterPrice = request.WaterPrice;
        contract.ServiceFee = request.ServiceFee;
        contract.InternetFee = request.InternetFee;
        contract.GarbageFee = request.GarbageFee;
        contract.BillingDate = request.BillingDate;
        contract.PaymentDueDays = request.PaymentDueDays;
        contract.MaxOccupants = request.MaxOccupants;
        contract.Terms = request.Terms;
        contract.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
