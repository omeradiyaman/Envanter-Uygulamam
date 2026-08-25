using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.Interfaces;

public interface IAssignmentDocumentRepository
{
    Task<IReadOnlyList<AssignmentDocument>> ListByPersonnelAsync(Guid personnelId, CancellationToken cancellationToken);
    Task<AssignmentDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(AssignmentDocument document, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
