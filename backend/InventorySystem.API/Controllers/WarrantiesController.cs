using InventorySystem.Application.DTOs;
using InventorySystem.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace InventorySystem.API.Controllers;

[ApiController]
[Authorize]
[Route("api/warranties")]
public sealed class WarrantiesController(ISender sender) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<WarrantySummaryDto>> GetSummary(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetWarrantySummaryQuery(), cancellationToken));

    [HttpGet("expiring")]
    public async Task<ActionResult<IReadOnlyList<WarrantyAlertDeviceDto>>> GetExpiring(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetExpiringWarrantiesQuery(), cancellationToken));

    [HttpGet("expired")]
    public async Task<ActionResult<IReadOnlyList<WarrantyAlertDeviceDto>>> GetExpired(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetExpiredWarrantiesQuery(), cancellationToken));

    [HttpGet("missing")]
    public async Task<ActionResult<IReadOnlyList<WarrantyAlertDeviceDto>>> GetMissing(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetDevicesWithoutWarrantyQuery(), cancellationToken));
}
