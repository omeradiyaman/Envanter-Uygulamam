using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.Interfaces;

public interface IPersonnelRepository
{
    Task<IReadOnlyList<Personnel>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Personnel>> ListFilteredAsync(
        string? searchTerm,
        bool? aktifMi,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Personnel>> ListForImportAsync(CancellationToken cancellationToken);

    Task<Personnel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> SicilNoExistsAsync(
        string sicilNo,
        Guid? excludingId,
        CancellationToken cancellationToken);

    Task AddAsync(Personnel personnel, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
