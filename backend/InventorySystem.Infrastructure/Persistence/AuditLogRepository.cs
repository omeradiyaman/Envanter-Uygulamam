using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Persistence;

public sealed class AuditLogRepository(ApplicationDbContext dbContext) : IAuditLogRepository
{
    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken) =>
        dbContext.AuditLogs.AddAsync(auditLog, cancellationToken).AsTask();

    public Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.AuditLogs.FirstOrDefaultAsync(log => log.Id == id, cancellationToken);

    public async Task<IReadOnlyList<AuditLog>> ListAsync(
        string? actionType,
        Guid? userId,
        bool systemUserOnly,
        string? entityType,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(actionType)) query = query.Where(log => log.ActionType == actionType);
        if (systemUserOnly) query = query.Where(log => log.UserId == null);
        else if (userId.HasValue) query = query.Where(log => log.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(entityType)) query = query.Where(log => log.EntityType == entityType);
        if (from.HasValue) query = query.Where(log => log.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(log => log.CreatedAt <= to.Value);

        return await query.OrderByDescending(log => log.CreatedAt).Take(500).ToListAsync(cancellationToken);
    }

    public Task<Personnel?> GetPersonnelIncludingDeletedAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Personnel.IgnoreQueryFilters().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<Device?> GetDeviceIncludingDeletedAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Devices.IgnoreQueryFilters().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<AssignmentHistory?> GetAssignmentAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.AssignmentHistories.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<bool> HasActiveDevicesAsync(Guid personnelId, CancellationToken cancellationToken) =>
        dbContext.Devices.AnyAsync(device => device.PersonelId == personnelId, cancellationToken);

    public Task<bool> SicilNoExistsAsync(string sicilNo, Guid excludingId, CancellationToken cancellationToken) =>
        dbContext.Personnel.AnyAsync(item => item.Id != excludingId && item.SicilNo == sicilNo, cancellationToken);

    public Task<bool> SerialOrInventoryExistsAsync(
        string serialNo,
        string inventoryNo,
        Guid excludingId,
        CancellationToken cancellationToken) =>
        dbContext.Devices.AnyAsync(
            item => item.Id != excludingId && (item.SeriNo == serialNo || item.EnvanterNo == inventoryNo),
            cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
