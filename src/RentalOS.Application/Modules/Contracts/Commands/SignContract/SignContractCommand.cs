using MediatR;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Contracts.Commands.SignContract;

public record SignContractCommand(Guid ContractId) : IRequest<Result<bool>>;

public class SignContractCommandHandler(IApplicationDbContext context) : IRequestHandler<SignContractCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(SignContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await context.Contracts.FindAsync([request.ContractId], cancellationToken);
        if (contract == null) return Result<bool>.Fail("CONTRACT_NOT_FOUND", "Không tìm thấy hợp đồng.");

        contract.SignedByCustomer = true;
        contract.SignedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true);
    }
}
