using MediatR;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Application.Modules.Customers.Dtos;

namespace RentalOS.Application.Modules.Customers.Commands.OcrIdCard;

public record OcrIdCardCommand : IRequest<Result<OcrResultDto>>
{
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

public class OcrIdCardCommandHandler : IRequestHandler<OcrIdCardCommand, Result<OcrResultDto>>
{
    private readonly IOcrService _ocrService;

    public OcrIdCardCommandHandler(IOcrService ocrService)
    {
        _ocrService = ocrService;
    }

    public async Task<Result<OcrResultDto>> Handle(OcrIdCardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _ocrService.ExtractIdCardInfoAsync(request.FileStream, cancellationToken);
            return Result<OcrResultDto>.Ok(result);
        }
        catch (Exception ex)
        {
            return Result<OcrResultDto>.Fail("OCR_FAILED", $"Lỗi khi trích xuất thông tin: {ex.Message}");
        }
    }
}
