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
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public decimal? MonthlyRent { get; init; }
    public decimal? DepositAmount { get; init; }
    public decimal? ElectricityPrice { get; init; }
    public decimal? WaterPrice { get; init; }
    public decimal? ServiceFee { get; init; }
    public decimal? InternetFee { get; init; }
    public decimal? GarbageFee { get; init; }
    public int? BillingDate { get; init; }
    public int? PaymentDueDays { get; init; }
    public int? MaxOccupants { get; init; }
    public string? Terms { get; init; }
    public ContractStatus? Status { get; init; }
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

        if (request.StartDate.HasValue) contract.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) contract.EndDate = request.EndDate.Value;
        if (request.MonthlyRent.HasValue) contract.MonthlyRent = request.MonthlyRent.Value;
        if (request.DepositAmount.HasValue) contract.DepositAmount = request.DepositAmount.Value;
        if (request.ElectricityPrice.HasValue) contract.ElectricityPrice = request.ElectricityPrice.Value;
        if (request.WaterPrice.HasValue) contract.WaterPrice = request.WaterPrice.Value;
        if (request.ServiceFee.HasValue) contract.ServiceFee = request.ServiceFee.Value;
        if (request.InternetFee.HasValue) contract.InternetFee = request.InternetFee.Value;
        if (request.GarbageFee.HasValue) contract.GarbageFee = request.GarbageFee.Value;
        if (request.BillingDate.HasValue) contract.BillingDate = request.BillingDate.Value;
        if (request.PaymentDueDays.HasValue) contract.PaymentDueDays = request.PaymentDueDays.Value;
        if (request.MaxOccupants.HasValue) contract.MaxOccupants = request.MaxOccupants.Value;
        if (request.Terms is not null) contract.Terms = request.Terms;
        if (request.Status.HasValue) contract.Status = request.Status.Value;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
