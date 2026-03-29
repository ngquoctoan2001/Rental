using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.Staff.Commands.UpdateStaff;

public record UpdateStaffCommand(Guid StaffId, string Role, List<Guid> AssignedPropertyIds) : IRequest<bool>;

public class UpdateStaffCommandHandler(
    UserManager<User> userManager,
    ITenantContext tenantContext) : IRequestHandler<UpdateStaffCommand, bool>
{
    public async Task<bool> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.Id == request.StaffId && u.TenantId == tenantContext.TenantId, cancellationToken);

        if (user == null)
        {
            throw new Exception("STAFF_NOT_FOUND");
        }

        user.Role = request.Role;
        user.AssignedPropertyIds = request.AssignedPropertyIds ?? [];
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return true;
    }
}
