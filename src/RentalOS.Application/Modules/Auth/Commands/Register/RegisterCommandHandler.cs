using System.Text.RegularExpressions;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.Auth.DTOs;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly ITenantSchemaManager _schemaManager;
    private readonly IJwtService _jwtService;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IApplicationDbContext _context; // Use IApplicationDbContext via DI

    public RegisterCommandHandler(
        ITenantSchemaManager schemaManager,
        IJwtService jwtService,
        UserManager<User> userManager,
        RoleManager<ApplicationRole> roleManager,
        IApplicationDbContext context)
    {
        _schemaManager = schemaManager;
        _jwtService = jwtService;
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate (already done by FluentValidation)

        // 2. Check if email/tenant slug exists in public.tenants
        var emailExists = await _context.Tenants.AnyAsync(t => t.OwnerEmail == request.OwnerEmail, cancellationToken);
        if (emailExists) throw new Exception("Email đã tồn tại.");

        // 3. Generate slug
        var slug = GenerateSlug(request.TenantName);
        var originalSlug = slug;
        int attempts = 0;
        while (await _context.Tenants.AnyAsync(t => t.Slug == slug, cancellationToken))

        {
            slug = $"{originalSlug}-{new Random().Next(1000, 9999)}";
            if (++attempts > 10) throw new Exception("Không thể tạo slug duy nhất.");
        }

        // 5. Create Tenant in public schema
        var tenant = new Tenant
        {
            Name = request.TenantName,
            Slug = slug,
            OwnerEmail = request.OwnerEmail,
            OwnerName = request.OwnerName,
            Phone = request.Phone,
            Plan = PlanType.Trial,
            TrialEndsAt = DateTime.UtcNow.AddDays(14),
            SchemaName = $"tenant_{slug.Replace("-", "_")}",
            IsActive = true
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);


        // 6 & 7. Create Schema and run DDL
        await _schemaManager.CreateSchemaAsync(slug, cancellationToken);

        // 8. Create User in NEW schema
        // We need to switch search_path to the new schema
        await _context.Database.ExecuteSqlRawAsync($"SET search_path TO \"{tenant.SchemaName}\"");

        var user = new User
        {
            UserName = request.OwnerEmail,
            Email = request.OwnerEmail,
            FullName = request.OwnerName,
            Phone = request.Phone,
            TenantId = tenant.Id,
            Role = "owner",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new Exception($"Lỗi tạo user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // 5. (Back to step 5 from prompt: Gán Role 'owner')
        // We need to ensure the role exists in the new schema
        if (!await _roleManager.RoleExistsAsync("owner"))
        {
            await _roleManager.CreateAsync(new ApplicationRole { Name = "owner" });
        }
        await _userManager.AddToRoleAsync(user, "owner");

        // 9. INSERT settings (already done by TenantSchemaManager DDL template)
        
        // 10. Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(
            user.Id.ToString(), 
            tenant.Slug, 
            "owner", 
            user.Email!, 
            tenant.Plan.ToString());
            
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        // 11. Save refresh token to Redis (Skip for now, or use ICacheService)

        // 12. Send welcome email (Skip for now or simple log)

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = "owner"
            },
            Tenant = new TenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug,
                Plan = tenant.Plan.ToString(),
                TrialEndsAt = tenant.TrialEndsAt
            }
        };
    }

    private string GenerateSlug(string text)
    {
        string str = RemoveAccents(text).ToLower();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", " ").Trim();
        str = str.Replace(" ", "-");
        return str;
    }

    private string RemoveAccents(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        text = text.Normalize(NormalizationForm.FormD);
        char[] chars = text.Where(c => char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).Normalize(NormalizationForm.FormC);
    }
}
