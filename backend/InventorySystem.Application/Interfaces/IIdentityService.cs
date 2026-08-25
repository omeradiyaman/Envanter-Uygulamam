using InventorySystem.Application.DTOs;

namespace InventorySystem.Application.Interfaces;

public interface IIdentityService
{
    Task<AuthUserDto?> PasswordSignInAsync(string userName, string password, CancellationToken cancellationToken);
    Task SignOutAsync();
    Task<AuthUserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserListItemDto>> ListUsersAsync(CancellationToken cancellationToken);
    Task<UserListItemDto> CreateUserAsync(string userName, string fullName, string password, string role, bool isActive, CancellationToken cancellationToken);
    Task<UserListItemDto> UpdateUserAsync(Guid id, string userName, string fullName, string role, bool isActive, CancellationToken cancellationToken);
    Task ResetPasswordAsync(Guid id, string newPassword, CancellationToken cancellationToken);
}
