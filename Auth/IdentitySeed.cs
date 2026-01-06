using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LitackaApi.Data;

namespace LitackaApi.Auth;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider sp, IConfiguration cfg)
    {
        using var scope = sp.CreateScope();

        // 1) DB migrace (SQLite)
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        // 2) Role
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 3) Super-admin z configu
        var email = cfg["BootstrapAdmin:Email"];
        var password = cfg["BootstrapAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        } 

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is null)
        {
            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", create.Errors.Select(e => e.Description)));
            }

            await userManager.AddToRoleAsync(user, Roles.SuperAdmin);
        }
        else
        {
            if (!await userManager.IsInRoleAsync(existing, Roles.SuperAdmin))
            {
                await userManager.AddToRoleAsync(existing, Roles.SuperAdmin);
            }
        }
    }
}
