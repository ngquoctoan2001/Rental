using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Common.Services;

public interface IPlanService
{
    int GetRoomLimit(PlanType plan);
    int GetPropertyLimit(PlanType plan);
    int GetStaffLimit(PlanType plan);
    bool HasFeature(PlanType plan, string feature);
    bool IsTrialExpired(Tenant tenant);
    bool IsPlanExpired(Tenant tenant);
}

public class PlanService : IPlanService
{
    public int GetRoomLimit(PlanType plan) => plan switch
    {
        PlanType.Starter => 20,
        PlanType.Pro => 100,
        PlanType.Business => int.MaxValue,
        _ => 20 // Trial/Default
    };

    public int GetPropertyLimit(PlanType plan) => plan switch
    {
        PlanType.Starter => 1,
        PlanType.Pro => 3,
        PlanType.Business => int.MaxValue,
        _ => 1
    };

    public int GetStaffLimit(PlanType plan) => plan switch
    {
        PlanType.Starter => 2,
        PlanType.Pro => 5,
        PlanType.Business => int.MaxValue,
        _ => 2
    };

    public bool HasFeature(PlanType plan, string feature)
    {
        return feature.ToLower() switch
        {
            "ai" => plan is PlanType.Pro or PlanType.Business,
            "reports" => plan is PlanType.Pro or PlanType.Business,
            "api" => plan is PlanType.Business,
            _ => true
        };
    }

    public bool IsTrialExpired(Tenant tenant) 
        => tenant.Plan == PlanType.Trial && tenant.TrialEndsAt < DateTime.UtcNow;

    public bool IsPlanExpired(Tenant tenant) 
        => tenant.Plan != PlanType.Trial && tenant.PlanExpiresAt < DateTime.UtcNow;
}
