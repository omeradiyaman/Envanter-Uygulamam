using InventorySystem.Application.Common.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InventorySystem.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedIdentityAsync(this IServiceProvider services, IConfiguration configuration)
    {
        await using var scope = services.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationUser>>();
        foreach (var role in ApplicationRoles.All)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));

        if (await userManager.GetUsersInRoleAsync(ApplicationRoles.Admin) is { Count: > 0 }) return;
        var userName = configuration["IdentitySeed:AdminUserName"] ?? "admin";
        var password = configuration["IdentitySeed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("İlk yönetici için IdentitySeed:AdminPassword güvenli environment secret olarak ayarlanmalıdır.");
        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(), UserName = userName, FullName = "Sistem Yöneticisi",
            IsActive = true, EmailConfirmed = true, LockoutEnabled = true
        };
        var result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        await userManager.AddToRoleAsync(admin, ApplicationRoles.Admin);
        logger.LogWarning("İlk yönetici hesabı oluşturuldu. Üretim ortamında başlangıç şifresini hemen değiştirin.");
    }
}
