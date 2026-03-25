using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.CoTenants.Commands.RemoveCoTenant;

public record RemoveCoTenantCommand(Guid Id) : IRequest<Result<bool>>;

public class RemoveCoTenantCommandHandler : IRequestHandler<RemoveCoTenantCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public RemoveCoTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(RemoveCoTenantCommand request, CancellationToken cancellationToken)
    {
        var coTenant = await _context.ContractCoTenants
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        if (coTenant == null)
        {
            return Result<bool>.Fail("COTENANT_NOT_FOUND", "Không tìm thấy bản ghi người ở cùng.");
        }

        if (coTenant.IsPrimary)
        {
            return Result<bool>.Fail("CANNOT_REMOVE_PRIMARY", "Không thể xóa khách thuê chính khỏi hợp đồng.");
        }

        _context.ContractCoTenants.Remove(coTenant);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
