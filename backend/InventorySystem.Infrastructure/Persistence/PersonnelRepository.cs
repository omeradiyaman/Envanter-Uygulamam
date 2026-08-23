using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Persistence;

public sealed class PersonnelRepository(ApplicationDbContext dbContext)
    : IPersonnelRepository
{
    public async Task<IReadOnlyList<Personnel>> ListAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Personnel
            .AsNoTracking()
            .OrderBy(personnel => personnel.Ad)
            .ThenBy(personnel => personnel.Soyad)
            .ThenBy(personnel => personnel.SicilNo)
            .ToListAsync(cancellationToken);
    }

    public Task<Personnel?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return dbContext.Personnel
            .FirstOrDefaultAsync(personnel => personnel.Id == id, cancellationToken);
    }

    public Task<bool> SicilNoExistsAsync(
        string sicilNo,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var normalizedSicilNo = sicilNo.Trim().ToUpperInvariant();

        return dbContext.Personnel.AnyAsync(
            personnel =>
                personnel.SicilNo == normalizedSicilNo
                && (!excludingId.HasValue || personnel.Id != excludingId.Value),
            cancellationToken);
    }

    public Task AddAsync(
        Personnel personnel,
        CancellationToken cancellationToken)
    {
        return dbContext.Personnel.AddAsync(personnel, cancellationToken).AsTask();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
