using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Infrastructure.Services.Storage;

public class R2StorageService : IR2StorageService
{
    private readonly IAmazonS3? _s3Client;
    private readonly string _bucketName;
    private readonly string _publicUrl;
    private readonly ILogger<R2StorageService> _logger;
    private readonly bool _isConfigured;

    public R2StorageService(IConfiguration configuration, ILogger<R2StorageService> logger)
    {
        _logger = logger;

        var section = configuration.GetSection("CloudflareR2");
        _bucketName = section["BucketName"] ?? "not-configured";
        _publicUrl = section["PublicUrl"] ?? "";

        var serviceUrl = section["ServiceUrl"] ?? "";

        // Guard: skip initialization if placeholder or empty URL is set
        if (string.IsNullOrWhiteSpace(serviceUrl)
            || serviceUrl.Contains("<account-id>")
            || !Uri.TryCreate(serviceUrl, UriKind.Absolute, out _))
        {
            _logger.LogWarning(
                "CloudflareR2:ServiceUrl is not configured or contains a placeholder ('{ServiceUrl}'). " +
                "R2StorageService will be unavailable. Set a valid URL in appsettings or environment variables.",
                serviceUrl);
            _isConfigured = false;
            return;
        }

        var config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true // R2 requires path-style addressing
        };

        _s3Client = new AmazonS3Client(
            section["AccessKey"],
            section["SecretKey"],
            config
        );
        _isConfigured = true;
    }

    private IAmazonS3 GetConfiguredClient()
    {
        if (!_isConfigured || _s3Client is null)
            throw new InvalidOperationException(
                "R2StorageService is not configured. Please set CloudflareR2:ServiceUrl, AccessKey, and SecretKey in your configuration.");
        return _s3Client;
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

            var fileTransferUtility = new TransferUtility(GetConfiguredClient());
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

        return await Task.Run(() => GetConfiguredClient().GetPreSignedURL(request));
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
        var url = await Task.Run(() => GetConfiguredClient().GetPreSignedURL(request));
        return (url, expiresAt);
    }

    public async Task DeleteAsync(string key)
    {
        try
        {
            await GetConfiguredClient().DeleteObjectAsync(_bucketName, key);
            _logger.LogInformation("Successfully deleted file from R2: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from R2: {Key}", key);
        }
    }
}
