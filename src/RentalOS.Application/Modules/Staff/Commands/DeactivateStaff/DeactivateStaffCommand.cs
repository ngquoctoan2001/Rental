using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.Staff.Commands.DeactivateStaff;

public record DeactivateStaffCommand(Guid StaffId) : IRequest<bool>;

public class DeactivateStaffCommandHandler(
    UserManager<User> userManager,
    ITenantContext tenantContext) : IRequestHandler<DeactivateStaffCommand, bool>
{
    public async Task<bool> Handle(DeactivateStaffCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.Id == request.StaffId && u.TenantId == tenantContext.TenantId, cancellationToken);

        if (user == null)
        {
            throw new Exception("STAFF_NOT_FOUND");
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return true;
    }
}
