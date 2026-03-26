using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;
using System.Linq;

namespace RentalOS.Application.Modules.Staff.Commands.AcceptInvite;

public record AcceptInviteCommand(
    string Token, 
    string Password, 
    string FullName, 
    string Phone) : IRequest<AuthResponseDto>;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class AcceptInviteCommandHandler(
    UserManager<User> userManager,
    IJwtService jwtService) : IRequestHandler<AcceptInviteCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(AcceptInviteCommand request, CancellationToken cancellationToken)
    {
        // 1. Lookup user by invite_token
        var user = userManager.Users.FirstOrDefault(u => u.InviteToken == request.Token);
        if (user == null)
            throw new Exception("Token không hợp lệ hoặc đã được sử dụng.");

        // 2. Check expiry
        if (user.InviteExpiresAt < DateTime.UtcNow)
            throw new Exception("Lời mời đã hết hạn (48h). Vui lòng yêu cầu Owner gửi lại.");

        // 3. Update User
        user.FullName = request.FullName;
        user.Phone = request.Phone;
        user.IsActive = true;
        user.InviteToken = null;
        user.InviteExpiresAt = null;

        // BCrypt manually as requested in some contexts, but let's use Identity standard
        // var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        // user.PasswordHash = passwordHash;
        
        // Actually AddPassword works better for Identity if PasswordHash is null
        var addPassResult = await userManager.AddPasswordAsync(user, request.Password);
        if (!addPassResult.Succeeded)
            throw new Exception(string.Join(", ", addPassResult.Errors.Select(e => e.Description)));

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new Exception("Lỗi khi cập nhật thông tin nhân viên.");

        // 4. Return auth tokens
        var token = jwtService.GenerateAccessToken(user.Id.ToString(), user.TenantId?.ToString() ?? string.Empty, user.Role, user.Email!, "trial");
        var refreshToken = jwtService.GenerateRefreshToken();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Role = user.Role
            }
        };
    }
}
