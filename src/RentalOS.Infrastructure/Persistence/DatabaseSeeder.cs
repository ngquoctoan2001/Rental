using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;

namespace RentalOS.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    private const string AdminSlug = "admin";
    private const string AdminEmail = "admin@rentalos.vn";
    private const string AdminPassword = "Admin@123456!";
    private const string ManagerEmail = "manager@rentalos.vn";
    private const string ManagerPassword = "Manager@123456!";
    private const string StaffEmail = "staff@rentalos.vn";
    private const string StaffPassword = "Staff@123456!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var schemaManager = serviceProvider.GetRequiredService<ITenantSchemaManager>();

        // 1. Seed Identity Roles
        foreach (var roleName in new[] { "owner", "manager", "staff" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new ApplicationRole { Name = roleName, Description = $"{roleName} role" });
        }

        // 2. Ensure demo tenant (admin@rentalos.vn) exists
        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == AdminSlug);
        if (tenant == null)
        {
            tenant = new Tenant
            {
                Slug = AdminSlug,
                Name = "RentalOS Demo",
                OwnerEmail = AdminEmail,
                OwnerName = "System Admin",
                Phone = "0900000000",
                Plan = PlanType.Business,
                PlanExpiresAt = DateTime.UtcNow.AddYears(10),
                SchemaName = $"tenant_{AdminSlug}",
                IsActive = true,
                OnboardingDone = true
            };
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();

            // 3. Create tenant schema for first-time admin tenant
            await schemaManager.CreateSchemaAsync(AdminSlug);
        }

        // 4. Ensure owner user in PUBLIC schema always has the expected identity and password
        //    (Do NOT switch search_path — UserManager must stay in the public schema.)
        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                NormalizedEmail = AdminEmail.ToUpperInvariant(),
                NormalizedUserName = AdminEmail.ToUpperInvariant(),
                FullName = "System Admin",
                Phone = "0900000000",
                Role = "owner",
                TenantId = tenant.Id,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(adminUser, AdminPassword);
            if (!createResult.Succeeded)
                throw new Exception($"Seeder: tạo user thất bại: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }
        else
        {
            adminUser.FullName = "System Admin";
            adminUser.Phone = "0900000000";
            adminUser.Role = "owner";
            adminUser.TenantId = tenant.Id;
            adminUser.IsActive = true;
            adminUser.EmailConfirmed = true;
            adminUser.UpdatedAt = DateTime.UtcNow;

            var updateResult = await userManager.UpdateAsync(adminUser);
            if (!updateResult.Succeeded)
                throw new Exception($"Seeder: cập nhật admin thất bại: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            var resetResult = await userManager.ResetPasswordAsync(adminUser, resetToken, AdminPassword);
            if (!resetResult.Succeeded)
                throw new Exception($"Seeder: reset mật khẩu admin thất bại: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
        }

        if (!await userManager.IsInRoleAsync(adminUser, "owner"))
        {
            await userManager.AddToRoleAsync(adminUser, "owner");
        }

        await EnsureDemoUser(userManager, tenant, "owner", AdminEmail, AdminPassword, "System Admin", "0900000000");
        await EnsureDemoUser(userManager, tenant, "manager", ManagerEmail, ManagerPassword, "Demo Manager", "0900000001");
        await EnsureDemoUser(userManager, tenant, "staff", StaffEmail, StaffPassword, "Demo Staff", "0900000002");

        // 5. Apply schema patches for ALL existing tenants
        var allTenants = await context.Tenants.ToListAsync();
        foreach (var t in allTenants)
        {
            await schemaManager.PatchSchemaAsync(t.Slug);
        }
    }

    private static async Task EnsureDemoUser(
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
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                throw new Exception($"Seeder: tạo user {role} thất bại: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
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
                throw new Exception($"Seeder: cập nhật user {role} thất bại: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, resetToken, password);
            if (!resetResult.Succeeded)
                throw new Exception($"Seeder: reset mật khẩu user {role} thất bại: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
