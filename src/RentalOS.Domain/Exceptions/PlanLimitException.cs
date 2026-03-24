namespace RentalOS.Domain.Exceptions;

/// <summary>Thrown when a tenant tries to exceed the resource limit imposed by their subscription plan.</summary>
public class PlanLimitException(string feature, int limit, string planName)
    : Exception($"Plan limit reached: '{feature}' allows {limit} on the '{planName}' plan.")
{
    public string Feature { get; } = feature;
    public int Limit { get; } = limit;
    public string PlanName { get; } = planName;
}
