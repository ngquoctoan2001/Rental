using RentalOS.Application.Modules.Customers.Dtos;

namespace RentalOS.Application.Common.Interfaces;

public interface IOcrService
{
    Task<OcrResultDto> ExtractIdCardInfoAsync(Stream imageStream, CancellationToken cancellationToken = default);
}
