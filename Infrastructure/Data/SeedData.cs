using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Constants;

namespace Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        foreach (var role in new[] { RoleNames.SuperAdmin, RoleNames.AdminStaff, RoleNames.Buyer })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (await userManager.FindByEmailAsync("superadmin@shopwala.pk") == null)
        {
            var superAdmin = new ApplicationUser
            {
                UserName = "superadmin@shopwala.pk",
                Email = "superadmin@shopwala.pk",
                EmailConfirmed = true,
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(superAdmin, "Admin@123!");
            await userManager.AddToRoleAsync(superAdmin, RoleNames.SuperAdmin);
        }

        if (await userManager.FindByEmailAsync("buyer@shopwala.pk") == null)
        {
            var buyer = new ApplicationUser
            {
                UserName = "buyer@shopwala.pk",
                Email = "buyer@shopwala.pk",
                EmailConfirmed = true,
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(buyer, "Buyer@123!");
            await userManager.AddToRoleAsync(buyer, RoleNames.Buyer);
        }
    }
}
