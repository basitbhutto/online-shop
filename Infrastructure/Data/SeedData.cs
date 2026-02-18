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


        #region Location 
        if (!context.Locations.Any())
        {
            // 1️⃣ Country
            var pakistan = new Location
            {
                Name = "Pakistan",
                ParentId = null,
                FullPath = "Pakistan",
                Latitude = 30.3753m,
                Longitude = 69.3451m,
                DisplayOrder = 1
            };

            context.Locations.Add(pakistan);
            await context.SaveChangesAsync();


            // 2️⃣ Province
            var sindh = new Location
            {
                Name = "Sindh",
                ParentId = pakistan.Id,
                FullPath = "Pakistan > Sindh",
                Latitude = 25.8943m,
                Longitude = 68.5247m,
                DisplayOrder = 1
            };

            context.Locations.Add(sindh);
            await context.SaveChangesAsync();


            // 3️⃣ City
            var karachi = new Location
            {
                Name = "Karachi",
                ParentId = sindh.Id,
                FullPath = "Pakistan > Sindh > Karachi",
                Latitude = 24.8607m,
                Longitude = 67.0011m,
                DisplayOrder = 1
            };

            context.Locations.Add(karachi);
            await context.SaveChangesAsync();


            // 4️⃣ Areas of Karachi
            var gulshan = new Location
            {
                Name = "Gulshan-e-Iqbal",
                ParentId = karachi.Id,
                FullPath = "Pakistan > Sindh > Karachi > Gulshan-e-Iqbal",
                Latitude = 24.9180m,
                Longitude = 67.0971m,
                DisplayOrder = 1
            };

            var northNazimabad = new Location
            {
                Name = "North Nazimabad",
                ParentId = karachi.Id,
                FullPath = "Pakistan > Sindh > Karachi > North Nazimabad",
                Latitude = 24.9400m,
                Longitude = 67.0450m,
                DisplayOrder = 2
            };

            var clifton = new Location
            {
                Name = "Clifton",
                ParentId = karachi.Id,
                FullPath = "Pakistan > Sindh > Karachi > Clifton",
                Latitude = 24.8138m,
                Longitude = 67.0295m,
                DisplayOrder = 3
            };

            var dha = new Location
            {
                Name = "DHA",
                ParentId = karachi.Id,
                FullPath = "Pakistan > Sindh > Karachi > DHA",
                Latitude = 24.8047m,
                Longitude = 67.0652m,
                DisplayOrder = 4
            };

            context.Locations.AddRange(gulshan, northNazimabad, clifton, dha);
            await context.SaveChangesAsync();


            // 5️⃣ Blocks under Gulshan
            var block1 = new Location
            {
                Name = "Block 1",
                ParentId = gulshan.Id,
                FullPath = "Pakistan > Sindh > Karachi > Gulshan-e-Iqbal > Block 1",
                Latitude = 24.9200m,
                Longitude = 67.0900m,
                DisplayOrder = 1
            };

            var block2 = new Location
            {
                Name = "Block 2",
                ParentId = gulshan.Id,
                FullPath = "Pakistan > Sindh > Karachi > Gulshan-e-Iqbal > Block 2",
                Latitude = 24.9215m,
                Longitude = 67.0920m,
                DisplayOrder = 2
            };

            var block3 = new Location
            {
                Name = "Block 3",
                ParentId = gulshan.Id,
                FullPath = "Pakistan > Sindh > Karachi > Gulshan-e-Iqbal > Block 3",
                Latitude = 24.9220m,
                Longitude = 67.0940m,
                DisplayOrder = 3
            };

            context.Locations.AddRange(block1, block2, block3);
            await context.SaveChangesAsync();
        }

        #endregion Location


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
