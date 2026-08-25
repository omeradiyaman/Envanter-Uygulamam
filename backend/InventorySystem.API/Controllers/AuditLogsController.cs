using InventorySystem.Application.Commands;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventorySystem.Application.Common.Security;

namespace InventorySystem.API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/audit-logs")]
public sealed class AuditLogsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetList(
        [FromQuery] string? actionType,
        [FromQuery] Guid? userId,
        [FromQuery] bool systemUserOnly,
        [FromQuery] string? entityType,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAuditLogsQuery(actionType, userId, systemUserOnly, entityType, from, to), cancellationToken));

    [HttpPost("{id:guid}/undo")]
    public async Task<ActionResult<UndoAuditLogResultDto>> Undo(Guid id, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new UndoAuditLogCommand(id), cancellationToken));
}
