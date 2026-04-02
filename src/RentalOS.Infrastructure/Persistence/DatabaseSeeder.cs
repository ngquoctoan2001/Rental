using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;

namespace RentalOS.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    private const string PlatformAdminSlug = "platform-admin";
    private const string PlatformAdminEmail = "admin@rentalos.vn";
    private const string PlatformAdminPassword = "Admin@123456!";

    private const string DemoLandlordSlug = "demo-landlord";
    private const string DemoLandlordEmail = "landlord@rentalos.vn";
    private const string DemoLandlordPassword = "Landlord@123456!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var schemaManager = serviceProvider.GetRequiredService<ITenantSchemaManager>();

        foreach (var roleName in new[] { "admin", "landlord" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    Description = $"{roleName} role",
                });
            }
        }

        var platformTenant = await EnsureTenant(
            context,
            schemaManager,
            PlatformAdminSlug,
            "RentalOS Platform",
            PlatformAdminEmail,
            "Platform Admin",
            "0900000000");

        var demoLandlordTenant = await EnsureTenant(
            context,
            schemaManager,
            DemoLandlordSlug,
            "Nhà trọ Demo",
            DemoLandlordEmail,
            "Demo Landlord",
            "0900000001");

        await EnsureUser(userManager, platformTenant, "admin", PlatformAdminEmail, PlatformAdminPassword, "Platform Admin", "0900000000");
        await EnsureUser(userManager, demoLandlordTenant, "landlord", DemoLandlordEmail, DemoLandlordPassword, "Demo Landlord", "0900000001");

        var allTenants = await context.Tenants.ToListAsync();
        foreach (var tenant in allTenants)
        {
            await schemaManager.PatchSchemaAsync(tenant.Slug);
        }
    }

    private static async Task<Tenant> EnsureTenant(
        ApplicationDbContext context,
        ITenantSchemaManager schemaManager,
        string slug,
        string name,
        string ownerEmail,
        string ownerName,
        string phone)
    {
        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == slug);
        if (tenant != null) return tenant;

        tenant = new Tenant
        {
            Slug = slug,
            Name = name,
            OwnerEmail = ownerEmail,
            OwnerName = ownerName,
            Phone = phone,
            Plan = PlanType.Business,
            PlanExpiresAt = DateTime.UtcNow.AddYears(10),
            SchemaName = $"tenant_{slug.Replace("-", "_")}",
            IsActive = true,
            OnboardingDone = true,
        };

        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        await schemaManager.CreateSchemaAsync(slug);
        return tenant;
    }

    private static async Task EnsureUser(
        UserManager<User> userManager,
        Tenant tenant,
        string role,
        string email,
        string password,
        string fullName,
        string phone)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                NormalizedUserName = email.ToUpperInvariant(),
                FullName = fullName,
                Phone = phone,
                Role = role,
                TenantId = tenant.Id,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new Exception($"Seeder: tạo user {role} thất bại: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            user.FullName = fullName;
            user.Phone = phone;
            user.Role = role;
            user.TenantId = tenant.Id;
            user.IsActive = true;
            user.EmailConfirmed = true;
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new Exception($"Seeder: cập nhật user {role} thất bại: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
            }

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, resetToken, password);
            if (!resetResult.Succeeded)
            {
                throw new Exception($"Seeder: reset mật khẩu user {role} thất bại: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
