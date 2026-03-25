using MediatR;
using Microsoft.AspNetCore.Http;
using RentalOS.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using RentalOS.Application.Modules.Settings.Dtos;

namespace RentalOS.Application.Modules.Settings.Commands.UploadLogo;

public record UploadLogoCommand(IFormFile File) : IRequest<string>;

public class UploadLogoCommandHandler(
    IApplicationDbContext dbContext,
    ITenantContext tenantContext,
    IMemoryCache cache,
    IFileStorageService storageService) : IRequestHandler<UploadLogoCommand, string>
{
    public async Task<string> Handle(UploadLogoCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate
        var ext = Path.GetExtension(request.File.FileName).ToLower();
        if (ext != ".jpg" && ext != ".png") throw new Exception("Chỉ hỗ trợ PNG/JPG.");
        if (request.File.Length > 2 * 1024 * 1024) throw new Exception("Dung lượng tối đa 2MB.");

        // 2. Upload to R2
        string path = $"logos/{tenantContext.TenantId}/logo{ext}";
        string url = await storageService.UploadAsync(request.File, path);

        // 3. Update Settings
        var setting = await dbContext.Settings.FirstOrDefaultAsync(s => s.Key == "company", cancellationToken);
        var company = setting != null 
            ? JsonSerializer.Deserialize<CompanySettingsDto>(setting.Value) ?? new() 
            : new CompanySettingsDto();

        company.LogoUrl = url;
        string json = JsonSerializer.Serialize(company);

        if (setting == null)
        {
            dbContext.Settings.Add(new Domain.Entities.Setting { Key = "company", Group = "System", Value = json });
        }
        else
        {
            setting.Value = json;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        cache.Remove($"settings:{tenantContext.TenantId}");

        return url;
    }
}
 Eskom file validation logic. Eskom R2 upload via IFileStorageService. Eskom background setting update.
