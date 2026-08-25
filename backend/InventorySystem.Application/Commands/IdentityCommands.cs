using InventorySystem.Application.DTOs;
using MediatR;

namespace InventorySystem.Application.Commands;

public sealed record LoginCommand(string UserName, string Password) : IRequest<AuthUserDto>;
public sealed record LogoutCommand : IRequest;
public sealed record CreateUserCommand(string UserName, string FullName, string Password, string Role, bool IsActive) : IRequest<UserListItemDto>;
public sealed record UpdateUserCommand(Guid Id, string UserName, string FullName, string Role, bool IsActive) : IRequest<UserListItemDto>;
public sealed record ResetUserPasswordCommand(Guid Id, string NewPassword) : IRequest;
