using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Persistence;

public sealed class AssignmentDocumentRepository(ApplicationDbContext dbContext)
    : IAssignmentDocumentRepository
{
    public async Task<IReadOnlyList<AssignmentDocument>> ListByPersonnelAsync(
        Guid personnelId,
        CancellationToken cancellationToken) =>
        await dbContext.AssignmentDocuments
            .Where(document => document.PersonnelId == personnelId)
            .OrderByDescending(document => document.UploadedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<AssignmentDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.AssignmentDocuments.FirstOrDefaultAsync(document => document.Id == id, cancellationToken);

    public Task AddAsync(AssignmentDocument document, CancellationToken cancellationToken) =>
        dbContext.AssignmentDocuments.AddAsync(document, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
