using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.Common.Security;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IDateTimeProvider dateTimeProvider) : IIdentityService
{
    public async Task<AuthUserDto?> PasswordSignInAsync(string userName, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(userName.Trim());
        if (user is null || !user.IsActive) return null;
        var result = await signInManager.PasswordSignInAsync(user, password, false, lockoutOnFailure: true);
        if (!result.Succeeded) return null;
        user.LastLoginAt = dateTimeProvider.UtcNow;
        await userManager.UpdateAsync(user);
        return await MapAuthAsync(user);
    }

    public Task SignOutAsync() => signInManager.SignOutAsync();

    public async Task<AuthUserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        return user is null || !user.IsActive ? null : await MapAuthAsync(user);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var ids = userIds.Distinct().ToArray();
        return await userManager.Users.AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.FullName, cancellationToken);
    }

    public async Task<IReadOnlyList<UserListItemDto>> ListUsersAsync(CancellationToken cancellationToken)
    {
        var users = await userManager.Users.AsNoTracking().OrderBy(user => user.UserName).ToListAsync(cancellationToken);
        var result = new List<UserListItemDto>(users.Count);
        foreach (var user in users) result.Add(await MapListAsync(user));
        return result;
    }

    public async Task<UserListItemDto> CreateUserAsync(
        string userName, string fullName, string password, string role, bool isActive,
        CancellationToken cancellationToken)
    {
        ValidateRole(role);
        if (await userManager.FindByNameAsync(userName.Trim()) is not null)
            throw new ConflictException("Bu kullanıcı adı zaten kullanılıyor.");
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), UserName = userName.Trim(), FullName = fullName.Trim(),
            IsActive = isActive, EmailConfirmed = true, LockoutEnabled = true
        };
        ThrowIfFailed(await userManager.CreateAsync(user, password));
        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            ThrowIfFailed(roleResult);
        }
        return await MapListAsync(user);
    }

    public async Task<UserListItemDto> UpdateUserAsync(
        Guid id, string userName, string fullName, string role, bool isActive,
        CancellationToken cancellationToken)
    {
        ValidateRole(role);
        var user = await userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("Kullanıcı bulunamadı.");
        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Contains(ApplicationRoles.Admin) && (role != ApplicationRoles.Admin || !isActive))
        {
            var adminCount = await CountActiveAdminsAsync(cancellationToken);
            if (adminCount <= 1) throw new BusinessRuleException("Son aktif yönetici pasifleştirilemez veya rolü değiştirilemez.");
        }
        var duplicate = await userManager.FindByNameAsync(userName.Trim());
        if (duplicate is not null && duplicate.Id != id) throw new ConflictException("Bu kullanıcı adı zaten kullanılıyor.");
        user.UserName = userName.Trim();
        user.FullName = fullName.Trim();
        user.IsActive = isActive;
        ThrowIfFailed(await userManager.UpdateAsync(user));
        if (!currentRoles.SequenceEqual([role]))
        {
            if (currentRoles.Count > 0) ThrowIfFailed(await userManager.RemoveFromRolesAsync(user, currentRoles));
            ThrowIfFailed(await userManager.AddToRoleAsync(user, role));
        }
        await userManager.UpdateSecurityStampAsync(user);
        return await MapListAsync(user);
    }

    public async Task ResetPasswordAsync(Guid id, string newPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("Kullanıcı bulunamadı.");
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        ThrowIfFailed(await userManager.ResetPasswordAsync(user, token, newPassword));
        await userManager.UpdateSecurityStampAsync(user);
    }

    private async Task<AuthUserDto> MapAuthAsync(ApplicationUser user)
    {
        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? ApplicationRoles.Viewer;
        return new AuthUserDto(user.Id, user.UserName ?? string.Empty, user.FullName, role, user.IsActive, user.LastLoginAt);
    }

    private async Task<UserListItemDto> MapListAsync(ApplicationUser user)
    {
        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? ApplicationRoles.Viewer;
        return new UserListItemDto(user.Id, user.UserName ?? string.Empty, user.FullName, role,
            user.IsActive, user.LastLoginAt, user.CreatedAt);
    }

    private async Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken)
    {
        var admins = await userManager.GetUsersInRoleAsync(ApplicationRoles.Admin);
        return admins.Count(user => user.IsActive);
    }

    private static void ValidateRole(string role)
    {
        if (!ApplicationRoles.All.Contains(role)) throw new BusinessRuleException("Geçersiz kullanıcı rolü.");
    }

    private static void ThrowIfFailed(IdentityResult result)
    {
        if (result.Succeeded) return;
        throw new BusinessRuleException(string.Join(" ", result.Errors.Select(error => error.Description)));
    }
}
