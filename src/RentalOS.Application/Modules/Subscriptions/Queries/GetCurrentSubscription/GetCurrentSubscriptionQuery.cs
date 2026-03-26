using MediatR;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Subscriptions.Queries.GetCurrentSubscription;

public record GetCurrentSubscriptionQuery : IRequest<SubscriptionDetailsDto>;

public class SubscriptionDetailsDto
{
    public PlanType Plan { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int DaysLeft { get; set; }
    public UsageDto Usage { get; set; } = new();
    public List<string> Features { get; set; } = new();
}

public class UsageDto
{
    public int RoomsUsed { get; set; }
    public int RoomsLimit { get; set; }
    public int PropertiesUsed { get; set; }
    public int PropertiesLimit { get; set; }
    public int StaffUsed { get; set; }
    public int StaffLimit { get; set; }
}

public class GetCurrentSubscriptionQueryHandler(
    IApplicationDbContext dbContext,
    ITenantContext tenantContext,
    IPlanService planService) : IRequestHandler<GetCurrentSubscriptionQuery, SubscriptionDetailsDto>
{
    public async Task<SubscriptionDetailsDto> Handle(GetCurrentSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FindAsync(new object[] { tenantContext.TenantId }, cancellationToken);
        if (tenant == null) throw new Exception("Tenant not found.");

        var roomsUsed = await dbContext.Rooms.CountAsync(cancellationToken);
        var propertiesUsed = await dbContext.Properties.CountAsync(cancellationToken);
        var staffUsed = await dbContext.Settings.CountAsync(s => s.Key == "staff_count_logic_placeholder"); // placeholder

        var expiresAt = tenant.Plan == PlanType.Trial ? tenant.TrialEndsAt : tenant.PlanExpiresAt;
        var daysLeft = expiresAt.HasValue ? (int)(expiresAt.Value - DateTime.UtcNow).TotalDays : 0;

        return new SubscriptionDetailsDto
        {
            Plan = tenantContext.Plan,
            ExpiresAt = expiresAt,
            DaysLeft = Math.Max(0, daysLeft),
            Usage = new UsageDto
            {
                RoomsUsed = roomsUsed,
                RoomsLimit = planService.GetRoomLimit(tenant.Plan),
                PropertiesUsed = propertiesUsed,
                PropertiesLimit = planService.GetPropertyLimit(tenant.Plan),
                StaffUsed = staffUsed,
                StaffLimit = planService.GetStaffLimit(tenant.Plan)
            },
            Features = new List<string>
            {
                planService.HasFeature(tenant.Plan, "ai") ? "AI Assistant" : "",
                planService.HasFeature(tenant.Plan, "reports") ? "Advanced Reports" : ""
            }.Where(s => !string.IsNullOrEmpty(s)).ToList()
        };
    }
}
