using MediatR;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Properties.Commands.UploadPropertyImage;

public record UploadPropertyImageCommand : IRequest<Result<string>>
{
    public Guid PropertyId { get; init; }
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public bool IsCover { get; init; } = false;
}
