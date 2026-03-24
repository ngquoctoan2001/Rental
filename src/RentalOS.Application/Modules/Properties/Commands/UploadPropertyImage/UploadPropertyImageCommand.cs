using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using System.Text.Json;

namespace RentalOS.Application.Modules.Properties.Commands.UploadPropertyImage;

public record UploadPropertyImageCommand : IRequest<string>
{
    public Guid Id { get; init; }
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

public class UploadPropertyImageCommandHandler : IRequestHandler<UploadPropertyImageCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IR2StorageService _storageService;

    public UploadPropertyImageCommandHandler(IApplicationDbContext context, IR2StorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<string> Handle(UploadPropertyImageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            throw new Exception("PROPERTY_NOT_FOUND");
        }

        var extension = Path.GetExtension(request.FileName);
        var key = $"properties/{request.Id}/{Guid.NewGuid()}{extension}";
        
        var url = await _storageService.UploadAsync(request.FileStream, key, request.ContentType, cancellationToken);

        // Update DB
        if (string.IsNullOrEmpty(entity.CoverImage))
        {
            entity.CoverImage = url;
        }
        else
        {
            var images = JsonSerializer.Deserialize<List<string>>(entity.Images) ?? new List<string>();
            images.Add(url);
            entity.Images = JsonSerializer.Serialize(images);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return url;
    }
}
