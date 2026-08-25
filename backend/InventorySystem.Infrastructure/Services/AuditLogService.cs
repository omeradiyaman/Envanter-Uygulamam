using System.Text.Json;
using System.Text.Json.Serialization;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;

namespace InventorySystem.Infrastructure.Services;

public sealed class AuditLogService(
    IAuditLogRepository repository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService) : IAuditLogService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<AuditLog> RecordAsync(
        string actionType,
        string entityType,
        Guid entityId,
        string description,
        object? oldValues,
        object? newValues,
        bool canUndo,
        CancellationToken cancellationToken,
        Guid? userId = null,
        Guid? operationId = null)
    {
        var log = new AuditLog(
            userId ?? currentUserService.UserId,
            actionType,
            entityType,
            entityId,
            description,
            oldValues is null ? null : JsonSerializer.Serialize(oldValues, JsonOptions),
            newValues is null ? null : JsonSerializer.Serialize(newValues, JsonOptions),
            dateTimeProvider.UtcNow,
            canUndo,
            operationId);
        await repository.AddAsync(log, cancellationToken);
        return log;
    }
}
