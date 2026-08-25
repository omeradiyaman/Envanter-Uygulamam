using InventorySystem.Domain.Common;

namespace InventorySystem.Domain.Entities;

public sealed class AssignmentHistory : BaseEntity
{
    private AssignmentHistory()
    {
    }

    public AssignmentHistory(
        Guid deviceId,
        Guid personnelId,
        DateTimeOffset assignedAt,
        string? note = null,
        Guid? assignedByUserId = null)
    {
        DeviceId = deviceId;
        PersonnelId = personnelId;
        AssignedAt = assignedAt;
        Note = note;
        AssignedByUserId = assignedByUserId;
    }

    public Guid DeviceId { get; private set; }
    public Device Device { get; private set; } = null!;

    public Guid PersonnelId { get; private set; }
    public Personnel Personnel { get; private set; } = null!;

    public DateTimeOffset AssignedAt { get; private set; }

    public DateTimeOffset? ReturnedAt { get; private set; }

    // For future Identity integration
    public Guid? AssignedByUserId { get; private set; }
    public Guid? ReturnedByUserId { get; private set; }

    public string? Note { get; private set; }

    public bool IsActive => ReturnedAt == null;

    public void MarkReturned(DateTimeOffset returnedAt, Guid? returnedByUserId = null, string? note = null)
    {
        ReturnedAt = returnedAt;
        ReturnedByUserId = returnedByUserId;
        if (note != null) Note = note;
        UpdatedAt = returnedAt;
    }

    public void UndoAssignment(DateTimeOffset undoneAt)
    {
        if (ReturnedAt.HasValue)
        {
            throw new InvalidOperationException("İade edilmiş zimmet geri alınamaz.");
        }

        ReturnedAt = undoneAt;
        IsDeleted = true;
        DeletedAt = undoneAt;
        UpdatedAt = undoneAt;
    }
}
