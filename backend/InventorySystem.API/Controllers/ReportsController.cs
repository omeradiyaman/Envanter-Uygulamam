using InventorySystem.Application.DTOs;
using InventorySystem.Application.Queries;
using InventorySystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportsController(ISender sender) : ControllerBase
{
    [HttpGet("inventory")]
    public async Task<ActionResult<InventoryReportDto>> Get(
        [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to,
        [FromQuery] Guid? categoryId, [FromQuery] string? department,
        [FromQuery] DeviceStatus? status, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetInventoryReportQuery(from, to, categoryId, department, status), cancellationToken));

    [HttpGet("inventory/export")]
    public async Task<IActionResult> Export(
        [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to,
        [FromQuery] Guid? categoryId, [FromQuery] string? department,
        [FromQuery] DeviceStatus? status, CancellationToken cancellationToken)
    {
        var file = await sender.Send(new ExportInventoryReportQuery(from, to, categoryId, department, status), cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
