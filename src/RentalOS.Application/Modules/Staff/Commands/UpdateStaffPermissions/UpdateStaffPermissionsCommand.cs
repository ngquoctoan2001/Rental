using MediatR;
using Microsoft.AspNetCore.Identity;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.Staff.Commands.UpdateStaffPermissions;

public record UpdateStaffPermissionsCommand(Guid StaffId, List<Guid> AssignedPropertyIds) : IRequest<bool>;

public class UpdateStaffPermissionsCommandHandler(UserManager<User> userManager)
    : IRequestHandler<UpdateStaffPermissionsCommand, bool>
{
    public async Task<bool> Handle(UpdateStaffPermissionsCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.StaffId.ToString());
        if (user == null) throw new Exception("Không tìm thấy nhân viên.");

        user.AssignedPropertyIds = request.AssignedPropertyIds;
        var result = await userManager.UpdateAsync(user);

        return result.Succeeded;
    }
}
