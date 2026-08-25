using InventorySystem.Domain.Entities;
namespace InventorySystem.Application.Interfaces;
public sealed record PersonnelPage(IReadOnlyList<Personnel> Items, int Page, int PageSize, int TotalCount, int TotalPages, int ActiveCount, int AssignedDeviceCount);
public interface IPersonnelRepository
{
    Task<PersonnelPage> ListAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);
    Task<Personnel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> SicilNoExistsAsync(string sicilNo, Guid? excludingId, CancellationToken cancellationToken);
    Task AddAsync(Personnel personnel, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
