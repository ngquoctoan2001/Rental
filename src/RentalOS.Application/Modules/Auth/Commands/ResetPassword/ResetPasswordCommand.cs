using MediatR;
using Microsoft.AspNetCore.Identity;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword, string TenantSlug) : IRequest<bool>;

public class ResetPasswordCommandHandler(UserManager<User> userManager) : IRequestHandler<ResetPasswordCommand, bool>
{
    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null) return false;

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return result.Succeeded;
    }
}
