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

        // 2. Seed demo tenant (admin@rentalos.vn) — skip if already exists
        var tenantExists = await context.Tenants.AnyAsync(t => t.Slug == AdminSlug);
        if (!tenantExists)
        {
            var tenant = new Tenant
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

            // 3. Create tenant schema
            await schemaManager.CreateSchemaAsync(AdminSlug);

            // 4. Create owner user in PUBLIC schema (Identity tables use PascalCase columns in public.users).
            //    Do NOT switch search_path — UserManager must stay in the public schema.
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
                var result = await userManager.CreateAsync(adminUser, AdminPassword);
                if (!result.Succeeded)
                    throw new Exception($"Seeder: tạo user thất bại: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                await userManager.AddToRoleAsync(adminUser, "owner");
            }
        }

        // 5. Apply schema patches for ALL existing tenants
        var allTenants = await context.Tenants.ToListAsync();
        foreach (var t in allTenants)
        {
            await schemaManager.PatchSchemaAsync(t.Slug);
        }
    }
}
