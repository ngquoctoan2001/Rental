using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Common.BackgroundJobs;

public class GenerateContractPdfJob(IContractPdfService pdfService)
{
    public async Task Execute(Guid contractId)
    {
        await pdfService.GenerateAndUploadContractPdfAsync(contractId, CancellationToken.None);
    }
}
