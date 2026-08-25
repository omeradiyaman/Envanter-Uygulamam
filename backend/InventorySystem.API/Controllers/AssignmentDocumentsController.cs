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
[Route("api/personnel/{personnelId:guid}/assignment-documents")]
public sealed class AssignmentDocumentsController(ISender sender) : ControllerBase
{
    [HttpGet("generated")]
    [Produces("application/pdf")]
    public async Task<IActionResult> Generate(
        Guid personnelId,
        [FromQuery] bool download = false,
        CancellationToken cancellationToken = default)
    {
        var document = await sender.Send(new GenerateAssignmentDocumentQuery(personnelId), cancellationToken);
        return download
            ? File(document.Content, document.ContentType, document.FileName)
            : File(document.Content, document.ContentType);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AssignmentDocumentDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssignmentDocumentDto>>> List(
        Guid personnelId,
        CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(new GetAssignmentDocumentsQuery(personnelId), cancellationToken));

    [HttpGet("{documentId:guid}/content")]
    public async Task<IActionResult> GetContent(
        Guid personnelId,
        Guid documentId,
        [FromQuery] bool download = false,
        CancellationToken cancellationToken = default)
    {
        var document = await sender.Send(
            new GetAssignmentDocumentFileQuery(personnelId, documentId),
            cancellationToken);
        return download
            ? File(document.Content, document.ContentType, document.FileName)
            : File(document.Content, document.ContentType);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.EditorOrAdmin)]
    [RequestSizeLimit(10 * 1024 * 1024 + 64 * 1024)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<AssignmentDocumentDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AssignmentDocumentDto>> Upload(
        Guid personnelId,
        [FromForm] UploadAssignmentDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var stream = new MemoryStream();
        await request.File.CopyToAsync(stream, cancellationToken);
        var document = await sender.Send(
            new UploadAssignmentDocumentCommand(
                personnelId,
                request.AssignmentHistoryId,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                stream.ToArray(),
                request.Description,
                request.ReplacesDocumentId),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetContent),
            new { personnelId, documentId = document.Id },
            document);
    }

    [HttpDelete("{documentId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid personnelId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        await sender.Send(new DeleteAssignmentDocumentCommand(personnelId, documentId), cancellationToken);
        return NoContent();
    }
}

public sealed class UploadAssignmentDocumentRequest
{
    public required IFormFile File { get; init; }
    public Guid? AssignmentHistoryId { get; init; }
    public string? Description { get; init; }
    public Guid? ReplacesDocumentId { get; init; }
}
