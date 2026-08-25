using InventorySystem.Application.DTOs;
using InventorySystem.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController(ISender sender) : ControllerBase
{
    [HttpGet("status")]
    [ProducesResponseType<SystemStatusDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemStatusDto>> GetStatus(
        [FromQuery] string client = "frontend",
        CancellationToken cancellationToken = default)
    {
        var status = await sender.Send(
            new GetSystemStatusQuery(client),
            cancellationToken);

        return Ok(status);
    }
}
