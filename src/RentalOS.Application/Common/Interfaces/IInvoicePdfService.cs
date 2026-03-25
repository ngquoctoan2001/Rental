namespace RentalOS.Application.Common.Interfaces;

public interface IInvoicePdfService
{
    Task<string> GenerateAndUploadInvoicePdfAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
