using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Persistence;

public sealed class AssignmentRepository(ApplicationDbContext dbContext)
    : IAssignmentRepository
{
    public Task<AssignmentHistory?> GetActiveAssignmentAsync(
        Guid deviceId,
        CancellationToken cancellationToken)
    {
        return dbContext.AssignmentHistories
            .Include(a => a.Device)
            .Include(a => a.Personnel)
            .FirstOrDefaultAsync(
                a => a.DeviceId == deviceId && a.ReturnedAt == null,
                cancellationToken);
    }

    public async Task<IReadOnlyList<AssignmentHistory>> GetDeviceHistoryAsync(
        Guid deviceId,
        CancellationToken cancellationToken)
    {
        return await dbContext.AssignmentHistories
            .Include(a => a.Device)
                .ThenInclude(d => d.Category)
            .Include(a => a.Personnel)
            .Where(a => a.DeviceId == deviceId)
            .OrderByDescending(a => a.AssignedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssignmentHistory>> GetPersonnelHistoryAsync(
        Guid personnelId,
        CancellationToken cancellationToken)
    {
        return await dbContext.AssignmentHistories
            .Include(a => a.Device)
                .ThenInclude(d => d.Category)
            .Include(a => a.Personnel)
            .Where(a => a.PersonnelId == personnelId)
            .OrderByDescending(a => a.AssignedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(AssignmentHistory assignment, CancellationToken cancellationToken)
    {
        return dbContext.AssignmentHistories.AddAsync(assignment, cancellationToken).AsTask();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
