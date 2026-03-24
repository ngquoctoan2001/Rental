namespace RentalOS.Application.Common.Interfaces;

public interface IR2StorageService
{
    Task<string> UploadAsync(Stream stream, string key, string contentType, CancellationToken ct = default);
    Task<string> GetPresignedUrlAsync(string key, int expiryMinutes = 60);
    Task DeleteAsync(string key);
}
