using InventorySystem.Application.Commands;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventorySystem.Application.Common.Security;

namespace InventorySystem.API.Controllers;

[ApiController]
[Authorize]
[Route("api/assignments")]
public sealed class AssignmentsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Assigns a device to a personnel member.
    /// </summary>
    [HttpPost("assign")]
    [Authorize(Policy = AuthorizationPolicies.EditorOrAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Assign(
        [FromBody] AssignDeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        await sender.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Returns (unassigns) a device from its current personnel.
    /// </summary>
    [HttpPost("unassign")]
    [Authorize(Policy = AuthorizationPolicies.EditorOrAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Unassign(
        [FromBody] UnassignDeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        await sender.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Returns the assignment history for a specific device.
    /// </summary>
    [HttpGet("device/{deviceId:guid}")]
    [ProducesResponseType<IReadOnlyList<AssignmentHistoryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssignmentHistoryDto>>> GetDeviceHistory(
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetDeviceAssignmentHistoryQuery(deviceId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns the assignment history for a specific personnel member.
    /// </summary>
    [HttpGet("personnel/{personnelId:guid}")]
    [ProducesResponseType<IReadOnlyList<AssignmentHistoryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssignmentHistoryDto>>> GetPersonnelHistory(
        Guid personnelId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetPersonnelAssignmentHistoryQuery(personnelId), cancellationToken);
        return Ok(result);
    }
}
