using InventorySystem.Application.Commands;
using InventorySystem.Application.Common.Security;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.API.Controllers;

[ApiController]
[Authorize]
[RequestSizeLimit(6 * 1024 * 1024)]
[Route("api/excel")]
public sealed class ExcelController(ISender sender) : ControllerBase
{
    [HttpGet("devices/export")]
    public async Task<IActionResult> ExportDevices([FromQuery] string? searchTerm, [FromQuery] Guid? categoryId,
        [FromQuery] InventorySystem.Domain.Enums.DeviceStatus? status, [FromQuery] Guid[]? ids,
        CancellationToken cancellationToken)
    {
        var file = await sender.Send(new ExportDevicesQuery(searchTerm, categoryId, status, ids), cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("personnel/export")]
    public async Task<IActionResult> ExportPersonnel([FromQuery] string? searchTerm, [FromQuery] bool? aktifMi,
        [FromQuery] Guid[]? ids, CancellationToken cancellationToken)
    {
        var file = await sender.Send(new ExportPersonnelQuery(searchTerm, aktifMi, ids), cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("inventory/export")]
    public async Task<IActionResult> ExportInventory(CancellationToken cancellationToken)
    {
        var file = await sender.Send(new ExportAllInventoryQuery(), cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [Authorize(Policy = AuthorizationPolicies.EditorOrAdmin)]
    [HttpPost("devices/preview")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImportPreviewSummaryDto>> PreviewDevices(IFormFile file, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new PreviewDeviceImportCommand(file.FileName, await ReadAsync(file, cancellationToken)), cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.EditorOrAdmin)]
    [HttpPost("devices/import")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImportExecutionResultDto>> ImportDevices(IFormFile file, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ImportDevicesCommand(file.FileName, await ReadAsync(file, cancellationToken)), cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.EditorOrAdmin)]
    [HttpPost("personnel/preview")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImportPreviewSummaryDto>> PreviewPersonnel(IFormFile file, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new PreviewPersonnelImportCommand(file.FileName, await ReadAsync(file, cancellationToken)), cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.EditorOrAdmin)]
    [HttpPost("personnel/import")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImportExecutionResultDto>> ImportPersonnel(IFormFile file, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ImportPersonnelCommand(file.FileName, await ReadAsync(file, cancellationToken)), cancellationToken));

    private static async Task<byte[]> ReadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        return stream.ToArray();
    }
}
