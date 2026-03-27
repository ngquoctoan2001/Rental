using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Onboarding.Commands.CompleteOnboarding;

public record CompleteOnboardingCommand : IRequest<bool>;

public class CompleteOnboardingCommandHandler(
    IApplicationDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<CompleteOnboardingCommand, bool>
{
    public async Task<bool> Handle(CompleteOnboardingCommand request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FindAsync(new object[] { tenantContext.TenantId }, cancellationToken)
            ?? throw new Exception("Tenant not found.");

        tenant.OnboardingDone = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
