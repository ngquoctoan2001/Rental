using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.CoTenants.Commands.UpdateCoTenant;

public record UpdateCoTenantCommand : IRequest<Result<bool>>
{
    public Guid Id { get; init; }
    public DateOnly? MovedInAt { get; init; }
    public DateOnly? MovedOutAt { get; init; }
}

public class UpdateCoTenantCommandHandler : IRequestHandler<UpdateCoTenantCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateCoTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateCoTenantCommand request, CancellationToken cancellationToken)
    {
        var coTenant = await _context.ContractCoTenants
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        if (coTenant == null)
        {
            return Result<bool>.Fail("COTENANT_NOT_FOUND", "Không tìm thấy bản ghi người ở cùng.");
        }

        if (request.MovedInAt.HasValue) coTenant.MovedInAt = request.MovedInAt.Value;
        if (request.MovedOutAt.HasValue) coTenant.MovedOutAt = request.MovedOutAt.Value;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
