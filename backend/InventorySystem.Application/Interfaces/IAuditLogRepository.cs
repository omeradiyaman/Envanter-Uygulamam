using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken);
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<AuditLog>> ListAsync(
        string? actionType,
        Guid? userId,
        bool systemUserOnly,
        string? entityType,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken);
    Task<Personnel?> GetPersonnelIncludingDeletedAsync(Guid id, CancellationToken cancellationToken);
    Task<Device?> GetDeviceIncludingDeletedAsync(Guid id, CancellationToken cancellationToken);
    Task<AssignmentHistory?> GetAssignmentAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> HasActiveDevicesAsync(Guid personnelId, CancellationToken cancellationToken);
    Task<bool> SicilNoExistsAsync(string sicilNo, Guid excludingId, CancellationToken cancellationToken);
    Task<bool> SerialOrInventoryExistsAsync(string serialNo, string inventoryNo, Guid excludingId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
