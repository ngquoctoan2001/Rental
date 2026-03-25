namespace RentalOS.Application.Common.Interfaces;

public interface IContractPdfService
{
    Task<string> GenerateAndUploadContractPdfAsync(Guid contractId, CancellationToken cancellationToken = default);
}
