using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.CoTenants.Commands.RemoveCoTenantByCustomer;

public record RemoveCoTenantByCustomerCommand(Guid ContractId, Guid CustomerId) : IRequest<Result<bool>>;

public class RemoveCoTenantByCustomerCommandHandler(IApplicationDbContext context) : IRequestHandler<RemoveCoTenantByCustomerCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RemoveCoTenantByCustomerCommand request, CancellationToken cancellationToken)
    {
        var coTenant = await context.ContractCoTenants
            .FirstOrDefaultAsync(ct => ct.ContractId == request.ContractId && ct.CustomerId == request.CustomerId, cancellationToken);

        if (coTenant == null) return Result<bool>.Fail("COTENANT_NOT_FOUND", "Không tìm thấy người ở cùng trong hợp đồng này.");
        if (coTenant.IsPrimary) return Result<bool>.Fail("CANNOT_REMOVE_PRIMARY", "Không thể xóa khách thuê chính.");

        context.ContractCoTenants.Remove(coTenant);
        await context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
