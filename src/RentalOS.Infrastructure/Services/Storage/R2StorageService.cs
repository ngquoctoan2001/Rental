using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Infrastructure.Services.Storage;

public class R2StorageService : IR2StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicUrl;
    private readonly ILogger<R2StorageService> _logger;

    public R2StorageService(IConfiguration configuration, ILogger<R2StorageService> logger)
    {
        _logger = logger;
        
        var section = configuration.GetSection("CloudflareR2");
        _bucketName = section["BucketName"] ?? throw new ArgumentNullException("R2 BucketName is missing");
        _publicUrl = section["PublicUrl"] ?? "";

        var config = new AmazonS3Config
        {
            ServiceURL = section["ServiceUrl"],
            ForcePathStyle = true // R2 requires this or it might try to use virtual-host style which can fail depending on region
        };

        _s3Client = new AmazonS3Client(
            section["AccessKey"],
            section["SecretKey"],
            config
        );
    }

    public async Task<string> UploadAsync(Stream stream, string key, string contentType, CancellationToken ct = default)
    {
        try
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = key,
                BucketName = _bucketName,
                ContentType = contentType,
                AutoCloseStream = true
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest, ct);

            _logger.LogInformation("Successfully uploaded file to R2: {Key}", key);
            return $"{_publicUrl}/{key}".Replace("//", "/").Replace("https:/", "https://");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to R2: {Key}", key);
            throw;
        }
    }

    public async Task<string> GetPresignedUrlAsync(string key, int expiryMinutes = 60)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };

        return await Task.Run(() => _s3Client.GetPreSignedURL(request));
    }

    public async Task<(string Url, DateTime ExpiresAt)> GetPresignedPutUrlAsync(string key, string contentType, TimeSpan expiry)
    {
        var expiresAt = DateTime.UtcNow.Add(expiry);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = expiresAt,
            ContentType = contentType
        };
        var url = await Task.Run(() => _s3Client.GetPreSignedURL(request));
        return (url, expiresAt);
    }

    public async Task DeleteAsync(string key)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(_bucketName, key);
            _logger.LogInformation("Successfully deleted file from R2: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from R2: {Key}", key);
        }
    }
}
