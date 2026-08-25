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
        return await CreatePersonnelQuery(asNoTracking: true)
            .OrderBy(personnel => personnel.Ad)
            .ThenBy(personnel => personnel.Soyad)
            .ThenBy(personnel => personnel.SicilNo)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Personnel>> ListFilteredAsync(
        string? searchTerm,
        bool? aktifMi,
        CancellationToken cancellationToken)
    {
        var query = CreatePersonnelQuery(asNoTracking: true);

        if (aktifMi.HasValue)
        {
            query = query.Where(personnel => personnel.AktifMi == aktifMi.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToUpperInvariant();

            query = query.Where(personnel =>
                personnel.SicilNo.ToUpper().Contains(normalized)
                || personnel.Ad.ToUpper().Contains(normalized)
                || personnel.Soyad.ToUpper().Contains(normalized)
                || personnel.Departman.ToUpper().Contains(normalized)
                || personnel.Pozisyon.ToUpper().Contains(normalized)
                || (personnel.ZimmetNo != null && personnel.ZimmetNo.ToUpper().Contains(normalized)));
        }

        return await query
            .OrderBy(personnel => personnel.Ad)
            .ThenBy(personnel => personnel.Soyad)
            .ThenBy(personnel => personnel.SicilNo)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Personnel>> ListForImportAsync(
        CancellationToken cancellationToken)
    {
        return await CreatePersonnelQuery(asNoTracking: false)
            .OrderBy(personnel => personnel.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Personnel?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return dbContext.Personnel
            .Include(p => p.Devices)
                .ThenInclude(d => d.Category)
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

    private IQueryable<Personnel> CreatePersonnelQuery(bool asNoTracking)
    {
        var query = dbContext.Personnel
            .Include(personnel => personnel.Devices)
            .AsQueryable();

        return asNoTracking ? query.AsNoTracking() : query;
    }
}
