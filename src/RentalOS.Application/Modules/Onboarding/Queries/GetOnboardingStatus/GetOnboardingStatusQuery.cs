using MediatR;
using RentalOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RentalOS.Application.Modules.Onboarding.Queries.GetOnboardingStatus;

public record GetOnboardingStatusQuery : IRequest<OnboardingStatusDto>;

public class OnboardingStatusDto
{
    public bool HasProfile { get; set; }
    public bool HasProperty { get; set; }
    public bool HasRoom { get; set; }
    public bool IsDone { get; set; }
    public int ProgressPercentage { get; set; }
}

public class GetOnboardingStatusQueryHandler(
    IApplicationDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<GetOnboardingStatusQuery, OnboardingStatusDto>
{
    public async Task<OnboardingStatusDto> Handle(GetOnboardingStatusQuery request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FindAsync(new object[] { tenantContext.TenantId }, cancellationToken);
        if (tenant == null) throw new Exception("Tenant not found.");

        var hasProperty = await dbContext.Properties.AnyAsync(cancellationToken);
        var hasRoom = await dbContext.Rooms.AnyAsync(cancellationToken);
        
        // Simple check for profile from settings
        var hasProfile = await dbContext.Settings.AnyAsync(s => s.Key == "company", cancellationToken);

        int completedSteps = (hasProfile ? 1 : 0) + (hasProperty ? 1 : 0) + (hasRoom ? 1 : 0);
        int progress = (int)((completedSteps / 3.0) * 100);

        return new OnboardingStatusDto
        {
            HasProfile = hasProfile,
            HasProperty = hasProperty,
            HasRoom = hasRoom,
            IsDone = tenant.OnboardingDone,
            ProgressPercentage = progress
        };
    }
}
 Eskom onboarding wizard status tracking. Eskom entity existence checks.
