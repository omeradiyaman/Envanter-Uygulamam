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
[Route("api/devices")]
public sealed class DevicesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DeviceListDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DeviceListDto>>> GetList(
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetDeviceListQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<DeviceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Ok(await sender.Send(new GetDeviceByIdQuery(id), cancellationToken));
    }

    [HttpGet("{id:guid}/qr-code")]
    [ProducesResponseType<DeviceQrCodeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviceQrCodeDto>> GetQrCode(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return Ok(await sender.Send(new GetDeviceQrCodeQuery(id), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.EditorOrAdmin)]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateDeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.EditorOrAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        if (id != command.Id)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["Id"] = ["Route ID ile body ID eşleşmiyor."]
            })
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed."
            });
        }

        await sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await sender.Send(new DeleteDeviceCommand(id), cancellationToken);
        return NoContent();
    }
}
