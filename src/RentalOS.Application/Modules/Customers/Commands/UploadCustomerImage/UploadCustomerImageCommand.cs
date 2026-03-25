using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Customers.Commands.UploadCustomerImage;

public record UploadCustomerImageCommand : IRequest<Result<string>>
{
    public Guid CustomerId { get; init; }
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public string ImageType { get; init; } = "portrait"; // portrait, id_front, id_back
}

public class UploadCustomerImageCommandHandler : IRequestHandler<UploadCustomerImageCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IR2StorageService _storageService;

    public UploadCustomerImageCommandHandler(IApplicationDbContext context, IR2StorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<Result<string>> Handle(UploadCustomerImageCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer == null)
        {
            return Result<string>.Fail("CUSTOMER_NOT_FOUND", "Không tìm thấy khách hàng.");
        }

        var extension = Path.GetExtension(request.FileName);
        var key = $"customers/{customer.Id}/{request.ImageType}_{Guid.NewGuid()}{extension}";

        var url = await _storageService.UploadAsync(request.FileStream, key, request.ContentType, cancellationToken);

        switch (request.ImageType.ToLower())
        {
            case "portrait":
                customer.PortraitImage = url;
                break;
            case "id_front":
                customer.IdCardImageFront = url;
                break;
            case "id_back":
                customer.IdCardImageBack = url;
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Ok(url);
    }
}
