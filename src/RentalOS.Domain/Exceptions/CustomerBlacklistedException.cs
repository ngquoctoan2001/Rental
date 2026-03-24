namespace RentalOS.Domain.Exceptions;

/// <summary>Thrown when an operation is attempted on a blacklisted customer.</summary>
public class CustomerBlacklistedException(Guid customerId, string? reason = null)
    : Exception($"Customer '{customerId}' is blacklisted and cannot perform this action. Reason: {reason ?? "not specified"}.")
{
    public Guid CustomerId { get; } = customerId;
    public string? Reason { get; } = reason;
}
