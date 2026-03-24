namespace RentalOS.Domain.Exceptions;

/// <summary>Thrown when a payment operation is attempted on an already fully-paid invoice.</summary>
public class InvoiceAlreadyPaidException(Guid invoiceId, string invoiceCode)
    : Exception($"Invoice '{invoiceCode}' ({invoiceId}) has already been paid in full.")
{
    public Guid InvoiceId { get; } = invoiceId;
    public string InvoiceCode { get; } = invoiceCode;
}
