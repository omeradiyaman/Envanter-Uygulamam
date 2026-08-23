using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.Interfaces;

public interface IPersonnelRepository
{
    Task<IReadOnlyList<Personnel>> ListAsync(CancellationToken cancellationToken);

    Task<Personnel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> SicilNoExistsAsync(
        string sicilNo,
        Guid? excludingId,
        CancellationToken cancellationToken);

    Task AddAsync(Personnel personnel, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
