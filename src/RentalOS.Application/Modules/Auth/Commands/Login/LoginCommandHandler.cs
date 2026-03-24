using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.Auth.DTOs;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IApplicationDbContext _context;

    public LoginCommandHandler(
        UserManager<User> userManager,
        IJwtService jwtService,
        IApplicationDbContext context)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _context = context;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Find tenant
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Slug == request.TenantSlug && t.IsActive, cancellationToken);

            
        if (tenant == null) throw new Exception("Không tìm thấy nhà trọ hoặc đã bị vô hiệu hóa.");

        // 2. SET search_path
        await _context.Database.ExecuteSqlRawAsync($"SET search_path TO \"{tenant.SchemaName}\"");

        // 3. Find user
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            throw new Exception("Email không tồn tại hoặc tài khoản bị khóa.");
        }

        // 4. Verify password
        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new Exception("Mật khẩu không chính xác.");
        }

        // 5. Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // 6. Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(
            user.Id.ToString(),
            tenant.Slug,
            user.Role,
            user.Email!,
            tenant.Plan.ToString());

        var refreshToken = _jwtService.GenerateRefreshToken();

        // 7. Save refresh token (Skip for now)

        return new AuthResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role
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
}
