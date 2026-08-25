using InventorySystem.Application.DTOs;
using InventorySystem.Application.Queries;
using InventorySystem.Application.Commands;
using InventorySystem.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace InventorySystem.API.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DeviceCategoryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DeviceCategoryDto>>> GetList(
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetDeviceCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.EditorOrAdmin)]
    public async Task<ActionResult<Guid>> Create(CreateDeviceCategoryCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.EditorOrAdmin)]
    public async Task<IActionResult> Update(Guid id, UpdateDeviceCategoryCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command with { Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteDeviceCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}
