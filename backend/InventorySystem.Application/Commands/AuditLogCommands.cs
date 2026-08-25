using InventorySystem.Application.DTOs;
using MediatR;

namespace InventorySystem.Application.Commands;

public sealed record UndoAuditLogCommand(Guid AuditLogId) : IRequest<UndoAuditLogResultDto>;
