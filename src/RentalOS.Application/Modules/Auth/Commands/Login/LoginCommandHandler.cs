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
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Slug == request.TenantSlug && t.IsActive, cancellationToken);

        if (tenant == null)
            throw new UnauthorizedAccessException("Không tìm thấy tenant hoặc tenant đã bị vô hiệu hóa.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Email không tồn tại hoặc tài khoản đang bị khóa.");

        var normalizedRole = (user.Role ?? string.Empty).ToLowerInvariant();
        var isSystemAdmin = normalizedRole == "admin";

        if (!isSystemAdmin && user.TenantId != tenant.Id)
            throw new UnauthorizedAccessException("Tài khoản không thuộc tenant này.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Mật khẩu không chính xác.");

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var accessToken = _jwtService.GenerateAccessToken(
            user.Id.ToString(),
            tenant.Slug,
            user.Role,
            user.Email!,
            tenant.Plan.ToString());

        var refreshToken = _jwtService.GenerateRefreshToken();

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
