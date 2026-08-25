using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Persistence;

public sealed class DeviceRepository(ApplicationDbContext dbContext)
    : IDeviceRepository
{
    public async Task<IReadOnlyList<Device>> ListAsync(
        CancellationToken cancellationToken)
    {
        return await CreateDeviceListQuery(asNoTracking: true)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Device>> ListFilteredAsync(
        string? searchTerm,
        Guid? categoryId,
        DeviceStatus? status,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilters(CreateDeviceListQuery(asNoTracking: true), searchTerm, categoryId, status);

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IReadOnlyList<DeviceCategory>> ListCategoriesAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.DeviceCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<DeviceCategory?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.DeviceCategories.FirstOrDefaultAsync(category => category.Id == id, cancellationToken);

    public Task<bool> CategoryNameExistsAsync(string name, Guid? excludingId, CancellationToken cancellationToken)
    {
        var normalized = name.Trim().ToUpperInvariant();
        return dbContext.DeviceCategories.AnyAsync(category =>
            category.Name.ToUpper() == normalized && (!excludingId.HasValue || category.Id != excludingId.Value), cancellationToken);
    }

    public Task<int> CategoryDeviceCountAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Devices.CountAsync(device => device.CategoryId == id, cancellationToken);

    public Task AddCategoryAsync(DeviceCategory category, CancellationToken cancellationToken) =>
        dbContext.DeviceCategories.AddAsync(category, cancellationToken).AsTask();

    public async Task<IReadOnlyList<Device>> ListForImportAsync(
        CancellationToken cancellationToken)
    {
        return await CreateDeviceListQuery(asNoTracking: false)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Device?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return dbContext.Devices
            .Include(d => d.Category)
            .Include(d => d.Personel)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public Task<bool> SerialNumberExistsAsync(
        string serialNumber,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var normalized = serialNumber.Trim().ToUpperInvariant();

        return dbContext.Devices.AnyAsync(
            d => d.SeriNo == normalized
                && (!excludingId.HasValue || d.Id != excludingId.Value),
            cancellationToken);
    }

    public Task<bool> InventoryNumberExistsAsync(
        string inventoryNumber,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var normalized = inventoryNumber.Trim().ToUpperInvariant();

        return dbContext.Devices.AnyAsync(
            d => d.EnvanterNo == normalized
                && (!excludingId.HasValue || d.Id != excludingId.Value),
            cancellationToken);
    }
    
    public Task<bool> CategoryExistsAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        return dbContext.DeviceCategories.AnyAsync(
            c => c.Id == categoryId,
            cancellationToken);
    }

    public Task AddAsync(
        Device device,
        CancellationToken cancellationToken)
    {
        return dbContext.Devices.AddAsync(device, cancellationToken).AsTask();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Device> CreateDeviceListQuery(bool asNoTracking)
    {
        var query = dbContext.Devices
            .Include(d => d.Category)
            .Include(d => d.Personel)
            .AsQueryable();

        return asNoTracking ? query.AsNoTracking() : query;
    }

    private static IQueryable<Device> ApplyFilters(
        IQueryable<Device> query,
        string? searchTerm,
        Guid? categoryId,
        DeviceStatus? status)
    {
        if (categoryId.HasValue)
        {
            query = query.Where(device => device.CategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(device => device.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToUpperInvariant();

            query = query.Where(device =>
                device.CihazAdi.ToUpper().Contains(normalized)
                || device.SeriNo.ToUpper().Contains(normalized)
                || device.EnvanterNo.ToUpper().Contains(normalized)
                || (device.BarkodNo != null && device.BarkodNo.ToUpper().Contains(normalized))
                || device.Marka.ToUpper().Contains(normalized)
                || device.Model.ToUpper().Contains(normalized)
                || device.Category.Name.ToUpper().Contains(normalized)
                || (device.Personel != null
                    && ((device.Personel.Ad + " " + device.Personel.Soyad).ToUpper().Contains(normalized)
                        || device.Personel.SicilNo.ToUpper().Contains(normalized))));
        }

        return query;
    }
}
