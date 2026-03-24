namespace RentalOS.Infrastructure.Services.Storage;

public class R2StorageService
{
    // TODO: integrate Cloudflare R2 via AWSSDK.S3 (S3-compatible)
    public Task<string> UploadAsync(Stream stream, string key, string contentType, CancellationToken ct = default)
        => Task.FromResult($"https://files.rentalos.vn/{key}");
}
