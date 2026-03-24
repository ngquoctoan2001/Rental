using MediatR;
using Microsoft.AspNetCore.Identity;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email, string TenantSlug) : IRequest<bool>;

public class ForgotPasswordCommandHandler(UserManager<User> userManager) : IRequestHandler<ForgotPasswordCommand, bool>
{
    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null) return false;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        // Log token for now (Simulated email sending)
        Console.WriteLine($"[Auth] Reset token for {request.Email}: {token}");
        
        return true;
    }
}
