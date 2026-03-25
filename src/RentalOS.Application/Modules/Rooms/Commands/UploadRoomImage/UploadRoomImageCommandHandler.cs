using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using System.Text.Json;

namespace RentalOS.Application.Modules.Rooms.Commands.UploadRoomImage;

public class UploadRoomImageCommandHandler : IRequestHandler<UploadRoomImageCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IR2StorageService _storageService;

    public UploadRoomImageCommandHandler(IApplicationDbContext context, IR2StorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<Result<string>> Handle(UploadRoomImageCommand request, CancellationToken cancellationToken)
    {
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken);

        if (room == null)
        {
            return Result<string>.Fail("ROOM_NOT_FOUND", "Không tìm thấy phòng.");
        }

        // Generate key: rooms/{id}/{uuid}.jpg
        var extension = Path.GetExtension(request.FileName);
        var key = $"rooms/{room.Id}/{Guid.NewGuid()}{extension}";

        // Upload to R2
        var url = await _storageService.UploadAsync(request.FileStream, key, request.ContentType, cancellationToken);

        // Update room images list
        var images = string.IsNullOrEmpty(room.Images) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(room.Images) ?? new List<string>();
        images.Add(url);
        room.Images = JsonSerializer.Serialize(images);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Ok(url);
    }
}
