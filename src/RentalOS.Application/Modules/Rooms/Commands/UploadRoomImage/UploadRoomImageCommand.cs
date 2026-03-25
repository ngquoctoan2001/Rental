using MediatR;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Rooms.Commands.UploadRoomImage;

public record UploadRoomImageCommand : IRequest<Result<string>>
{
    public Guid RoomId { get; init; }
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}
