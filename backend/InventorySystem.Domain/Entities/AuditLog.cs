using InventorySystem.Domain.Common;

namespace InventorySystem.Domain.Entities;

public sealed class AuditLog : BaseEntity
{
    private AuditLog() { }

    public AuditLog(
        Guid? userId,
        string actionType,
        string entityType,
        Guid entityId,
        string description,
        string? oldValues,
        string? newValues,
        DateTimeOffset createdAt,
        bool canUndo,
        Guid? operationId = null)
    {
        UserId = userId;
        ActionType = actionType;
        EntityType = entityType;
        EntityId = entityId;
        Description = description;
        OldValues = oldValues;
        NewValues = newValues;
        CreatedAt = createdAt;
        CanUndo = canUndo;
        OperationId = operationId;
    }

    public Guid? UserId { get; private set; }
    public string ActionType { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public bool CanUndo { get; private set; }
    public DateTimeOffset? UndoneAt { get; private set; }
    public Guid? UndoAuditLogId { get; private set; }
    public Guid? OperationId { get; private set; }

    public void MarkUndone(DateTimeOffset undoneAt, Guid undoAuditLogId)
    {
        if (!CanUndo || UndoneAt.HasValue)
        {
            throw new InvalidOperationException("Bu işlem geri alınamaz veya daha önce geri alınmış.");
        }

        CanUndo = false;
        UndoneAt = undoneAt;
        UndoAuditLogId = undoAuditLogId;
        UpdatedAt = undoneAt;
    }
}
