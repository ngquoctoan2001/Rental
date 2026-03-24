namespace RentalOS.Domain.Exceptions;

/// <summary>Thrown when a callback from a payment gateway (Momo/VNPay) cannot be verified (signature mismatch, replay attack, etc.).</summary>
public class PaymentVerificationException(string provider, string reason)
    : Exception($"Payment verification failed for provider '{provider}': {reason}.")
{
    public string Provider { get; } = provider;
    public string Reason { get; } = reason;
}
