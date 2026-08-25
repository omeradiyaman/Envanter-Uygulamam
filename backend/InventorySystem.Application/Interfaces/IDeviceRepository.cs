using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;

namespace InventorySystem.Application.Interfaces;

public interface IDeviceRepository
{
    Task<IReadOnlyList<Device>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Device>> ListFilteredAsync(
        string? searchTerm,
        Guid? categoryId,
        DeviceStatus? status,
        CancellationToken cancellationToken);
    
    Task<IReadOnlyList<DeviceCategory>> ListCategoriesAsync(CancellationToken cancellationToken);
    Task<DeviceCategory?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> CategoryNameExistsAsync(string name, Guid? excludingId, CancellationToken cancellationToken);
    Task<int> CategoryDeviceCountAsync(Guid id, CancellationToken cancellationToken);
    Task AddCategoryAsync(DeviceCategory category, CancellationToken cancellationToken);

    Task<IReadOnlyList<Device>> ListForImportAsync(CancellationToken cancellationToken);

    Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> SerialNumberExistsAsync(
        string serialNumber,
        Guid? excludingId,
        CancellationToken cancellationToken);

    Task<bool> InventoryNumberExistsAsync(
        string inventoryNumber,
        Guid? excludingId,
        CancellationToken cancellationToken);
        
    Task<bool> CategoryExistsAsync(
        Guid categoryId,
        CancellationToken cancellationToken);

    Task AddAsync(Device device, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
