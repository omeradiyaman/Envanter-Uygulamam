using InventorySystem.Application.DTOs;
using MediatR;

namespace InventorySystem.Application.Queries;

public sealed record GetAuditLogsQuery(
    string? ActionType = null,
    Guid? UserId = null,
    bool SystemUserOnly = false,
    string? EntityType = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null) : IRequest<IReadOnlyList<AuditLogDto>>;
