using InventorySystem.Application.Commands;
using InventorySystem.Application.Common.Security;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/users")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserListItemDto>>> List(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetUsersQuery(), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<UserListItemDto>> Create(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await sender.Send(command, cancellationToken);
        return Created($"/api/users/{user.Id}", user);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserListItemDto>> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new UpdateUserCommand(id, request.UserName, request.FullName, request.Role, request.IsActive), cancellationToken));

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new ResetUserPasswordCommand(id, request.NewPassword), cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateUserRequest(string UserName, string FullName, string Role, bool IsActive);
public sealed record ResetPasswordRequest(string NewPassword);
