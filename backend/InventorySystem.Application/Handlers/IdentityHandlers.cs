using InventorySystem.Application.Commands;
using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Queries;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class LoginCommandHandler(IIdentityService identityService) : IRequestHandler<LoginCommand, AuthUserDto>
{
    public async Task<AuthUserDto> Handle(LoginCommand request, CancellationToken cancellationToken) =>
        await identityService.PasswordSignInAsync(request.UserName, request.Password, cancellationToken)
        ?? throw new UnauthorizedAccessException("Kullanıcı adı veya şifre hatalı.");
}

public sealed class LogoutCommandHandler(IIdentityService identityService) : IRequestHandler<LogoutCommand>
{
    public Task Handle(LogoutCommand request, CancellationToken cancellationToken) => identityService.SignOutAsync();
}

public sealed class GetCurrentUserQueryHandler(ICurrentUserService currentUser, IIdentityService identityService)
    : IRequestHandler<GetCurrentUserQuery, AuthUserDto>
{
    public async Task<AuthUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.UserId.HasValue) throw new UnauthorizedAccessException();
        return await identityService.GetUserAsync(currentUser.UserId.Value, cancellationToken)
            ?? throw new UnauthorizedAccessException();
    }
}

public sealed class GetUsersQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUsersQuery, IReadOnlyList<UserListItemDto>>
{
    public Task<IReadOnlyList<UserListItemDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken) =>
        identityService.ListUsersAsync(cancellationToken);
}

public sealed class CreateUserCommandHandler(IIdentityService identityService)
    : IRequestHandler<CreateUserCommand, UserListItemDto>
{
    public Task<UserListItemDto> Handle(CreateUserCommand request, CancellationToken cancellationToken) =>
        identityService.CreateUserAsync(request.UserName, request.FullName, request.Password, request.Role, request.IsActive, cancellationToken);
}

public sealed class UpdateUserCommandHandler(IIdentityService identityService)
    : IRequestHandler<UpdateUserCommand, UserListItemDto>
{
    public Task<UserListItemDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken) =>
        identityService.UpdateUserAsync(request.Id, request.UserName, request.FullName, request.Role, request.IsActive, cancellationToken);
}

public sealed class ResetUserPasswordCommandHandler(IIdentityService identityService)
    : IRequestHandler<ResetUserPasswordCommand>
{
    public Task Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken) =>
        identityService.ResetPasswordAsync(request.Id, request.NewPassword, cancellationToken);
}
