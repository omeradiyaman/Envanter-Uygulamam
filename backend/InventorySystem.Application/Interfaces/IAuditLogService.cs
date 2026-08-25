using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.Interfaces;

public interface IAuditLogService
{
    Task<AuditLog> RecordAsync(
        string actionType,
        string entityType,
        Guid entityId,
        string description,
        object? oldValues,
        object? newValues,
        bool canUndo,
        CancellationToken cancellationToken,
        Guid? userId = null,
        Guid? operationId = null);
}
