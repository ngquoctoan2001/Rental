#pragma warning disable CS9113 // Parameter is reserved for future use
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace RentalOS.Application.Modules.Staff.Commands.InviteStaff;

public record InviteStaffCommand(string Email, string Role, List<Guid>? AssignedPropertyIds = null) : IRequest<Guid>;

public class InviteStaffCommandValidator : AbstractValidator<InviteStaffCommand>
{
    public InviteStaffCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Role).NotEmpty().Must(r => new[] { "manager", "staff" }.Contains(r.ToLower()));
    }
}

public class InviteStaffCommandHandler(
    UserManager<User> userManager,
    ITenantContext tenantContext,
    IApplicationDbContext dbContext,
    IBackgroundJobClient _backgroundJobClient) : IRequestHandler<InviteStaffCommand, Guid>
{
    public async Task<Guid> Handle(InviteStaffCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate Plan Limit
        var tenant = await dbContext.Tenants.FindAsync(new object[] { tenantContext.TenantId }, cancellationToken)
            ?? throw new Exception("Tenant not found");

        var currentStaffCount = userManager.Users.Count(u => u.TenantId == tenantContext.TenantId && u.Role != "owner");
        
        int limit = tenant.Plan switch
        {
            PlanType.Starter => 2,
            PlanType.Pro => 5,
            PlanType.Business => int.MaxValue,
            _ => 2 // Default Trial/Starter
        };

        if (currentStaffCount >= limit)
            throw new Exception($"Gói {tenant.Plan} giới hạn tối đa {limit} nhân viên. Vui lòng nâng cấp.");

        // 2. Check email uniqueness in tenant
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null && existingUser.TenantId == tenantContext.TenantId)
            throw new Exception("Email này đã được mời hoặc đang tham gia vào hệ thống.");

        // 3. Create User record (inactive)
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            Role = request.Role,
            TenantId = tenantContext.TenantId,
            IsActive = false,
            InviteToken = Guid.NewGuid().ToString("N"),
            InviteExpiresAt = DateTime.UtcNow.AddHours(48),
            AssignedPropertyIds = request.AssignedPropertyIds ?? new List<Guid>()
        };

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        // 4. Enqueue Job to send email
        // backgroundJobClient.Enqueue<IEmailService>(s => s.SendStaffInviteEmail(user.Id, user.Email, tenant.Slug));

        return user.Id;
    }
}
