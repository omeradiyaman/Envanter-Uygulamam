using InventorySystem.Application.DTOs;
using MediatR;

namespace InventorySystem.Application.Queries;

public sealed record GetCurrentUserQuery : IRequest<AuthUserDto>;
public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserListItemDto>>;
