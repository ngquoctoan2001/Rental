using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Properties.Commands.UploadPropertyImage;

public class UploadPropertyImageCommandHandler(
    IApplicationDbContext context,
    IR2StorageService storageService) : IRequestHandler<UploadPropertyImageCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UploadPropertyImageCommand request, CancellationToken cancellationToken)
    {
        var property = await context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken);

        if (property == null)
        {
            return Result<string>.Fail("PROPERTY_NOT_FOUND", "Không tìm thấy nhà trọ.");
        }

        // Generate key: properties/{id}/{uuid}{ext}
        var extension = Path.GetExtension(request.FileName);
        var key = $"properties/{property.Id}/{Guid.NewGuid()}{extension}";

        // Upload to R2
        var url = await storageService.UploadAsync(request.FileStream, key, request.ContentType, cancellationToken);

        if (request.IsCover)
        {
            property.CoverImage = url;
        }
        else
        {
            var images = System.Text.Json.JsonSerializer.Deserialize<List<string>>(property.Images) ?? [];
            images.Add(url);
            property.Images = System.Text.Json.JsonSerializer.Serialize(images);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result<string>.Ok(url);
    }
}
