using MediatR;
using QRCoder;
using Microsoft.Extensions.Configuration;

namespace RentalOS.Application.Modules.Rooms.Queries.GetRoomQrCode;

public record GetRoomQrCodeQuery(Guid Id) : IRequest<byte[]>;

public class GetRoomQrCodeQueryHandler : IRequestHandler<GetRoomQrCodeQuery, byte[]>
{
    private readonly IConfiguration _configuration;

    public GetRoomQrCodeQueryHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<byte[]> Handle(GetRoomQrCodeQuery request, CancellationToken cancellationToken)
    {
        var appUrl = _configuration["AppUrl"] ?? "http://localhost:3000";
        var content = $"{appUrl}/pay?room={request.Id}";

        using (var qrGenerator = new QRCodeGenerator())
        using (var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q))
        using (var qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            return Task.FromResult(qrCodeImage);
        }
    }
}
