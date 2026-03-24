namespace RentalOS.Domain.Enums;

/// <summary>Execution result of a transaction.</summary>
public enum TransactionStatus
{
    Success,
    Failed,
    Refunded,
    Pending
}
