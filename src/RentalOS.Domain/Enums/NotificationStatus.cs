namespace RentalOS.Domain.Enums;

/// <summary>Delivery status of a notification attempt.</summary>
public enum NotificationStatus
{
    Pending,
    Sent,
    Failed,
    Skipped,
    FailedPermanent
}
