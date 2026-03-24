namespace RentalOS.Domain.Enums;

/// <summary>Payment status of an invoice.</summary>
public enum InvoiceStatus
{
    Pending,
    Paid,
    Overdue,
    Cancelled,
    Partial
}
