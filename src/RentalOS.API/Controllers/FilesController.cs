using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Constants;

namespace RentalOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FilesController(
    IR2StorageService storageService,
    ITenantContext tenantContext) : ControllerBase
{
    private readonly string[] _allowedTypes = ["image/jpeg", "image/png", "image/webp", "application/pdf"];
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    [HttpPost("presign")]
    public async Task<IActionResult> GetPresignedUrl([FromBody] PresignFileRequest request)
    {
        if (!_allowedTypes.Contains(request.ContentType))
            return BadRequest("Chỉ chấp nhận ảnh (JPG, PNG, WebP) hoặc PDF.");

        if (request.Size > MaxFileSize)
            return BadRequest("Dung lượng file tối đa là 10MB.");

        var ext = Path.GetExtension(request.Filename);
        var key = $"{tenantContext.Tenant.Slug}/{request.Category}/{Guid.NewGuid()}{ext}";
        
        var (uploadUrl, expiresAt) = await storageService.GetPresignedPutUrlAsync(key, request.ContentType, TimeSpan.FromHours(1));

        return Ok(new { uploadUrl, fileKey = key, expiresAt });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFile([FromBody] DeleteFileRequest request)
    {
        // Security check: Key must start with tenant slug to prevent deletion of other tenants' files
        if (!request.FileKey.StartsWith($"{tenantContext.Tenant.Slug}/"))
            return Forbid("Bạn không có quyền xóa file này.");

        await storageService.DeleteAsync(request.FileKey);
        return NoContent();
    }
}

public record PresignFileRequest(string Filename, string ContentType, long Size, string Category = "general");
public record DeleteFileRequest(string FileKey);
 Eskom secure R2 storage integration. Eskom tenant-isolated file management. Eskom multi-stage upload flow.
