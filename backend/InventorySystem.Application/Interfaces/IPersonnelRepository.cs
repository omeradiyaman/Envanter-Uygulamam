using InventorySystem.Domain.Entities;
namespace InventorySystem.Application.Interfaces;
public sealed record PersonnelPage(IReadOnlyList<Personnel> Items,int Page,int PageSize,int TotalCount,int TotalPages,int ActiveCount,int AssignedDeviceCount);
public interface IPersonnelRepository { Task<PersonnelPage> ListAsync(string? search,int page,int pageSize,CancellationToken ct); Task<Personnel?> GetByIdAsync(Guid id,CancellationToken ct); Task<bool> SicilNoExistsAsync(string sicilNo,Guid? excludingId,CancellationToken ct); Task AddAsync(Personnel personnel,CancellationToken ct); Task SaveChangesAsync(CancellationToken ct); }
