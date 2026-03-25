using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Infrastructure.BackgroundJobs;

public class GenerateContractPdfJob(IContractPdfService pdfService)
{
    public async Task Execute(Guid contractId)
    {
        await pdfService.GenerateAndUploadContractPdfAsync(contractId, CancellationToken.None);
    }
}
