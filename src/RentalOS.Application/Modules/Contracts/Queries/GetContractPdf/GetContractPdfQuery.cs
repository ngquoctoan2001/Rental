using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Contracts.Queries.GetContractPdf;

public record GetContractPdfQuery(Guid ContractId) : IRequest<Result<string>>;

public class GetContractPdfQueryHandler(IApplicationDbContext context) : IRequestHandler<GetContractPdfQuery, Result<string>>
{
    public async Task<Result<string>> Handle(GetContractPdfQuery request, CancellationToken cancellationToken)
    {
        var contract = await context.Contracts
            .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

        if (contract == null) return Result<string>.Fail("CONTRACT_NOT_FOUND", "Không tìm thấy hợp đồng.");
        if (string.IsNullOrEmpty(contract.PdfUrl)) 
            return Result<string>.Fail("PDF_NOT_READY", "Tệp PDF chưa được tạo hoặc đang xử lý.");

        return Result<string>.Ok(contract.PdfUrl);
    }
}
