using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.Interfaces;

public interface IAssignmentRepository
{
    /// <summary>
    /// Gets the currently active assignment for a device (ReturnedAt == null).
    /// Returns null if the device has no active assignment.
    /// </summary>
    Task<AssignmentHistory?> GetActiveAssignmentAsync(Guid deviceId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns all assignment history records for a device, ordered by AssignedAt descending.
    /// </summary>
    Task<IReadOnlyList<AssignmentHistory>> GetDeviceHistoryAsync(Guid deviceId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns all assignment history records for a personnel member, ordered by AssignedAt descending.
    /// </summary>
    Task<IReadOnlyList<AssignmentHistory>> GetPersonnelHistoryAsync(Guid personnelId, CancellationToken cancellationToken);

    Task AddAsync(AssignmentHistory assignment, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
