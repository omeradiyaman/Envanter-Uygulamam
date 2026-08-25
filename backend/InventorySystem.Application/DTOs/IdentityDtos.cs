namespace InventorySystem.Application.DTOs;

public sealed record AuthUserDto(
    Guid Id,
    string UserName,
    string FullName,
    string Role,
    bool IsActive,
    DateTimeOffset? LastLoginAt);

public sealed record UserListItemDto(
    Guid Id,
    string UserName,
    string FullName,
    string Role,
    bool IsActive,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt);
