using InventorySystem.Domain.Common;

namespace InventorySystem.Domain.Entities;

public sealed class AssignmentDocument : BaseEntity
{
    private AssignmentDocument() { }

    public AssignmentDocument(
        Guid personnelId,
        Guid? assignmentHistoryId,
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        DateTimeOffset uploadedAt,
        string? description,
        Guid? replacesDocumentId)
    {
        PersonnelId = personnelId;
        AssignmentHistoryId = assignmentHistoryId;
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        ContentType = contentType;
        FileSize = fileSize;
        UploadedAt = uploadedAt;
        Description = description;
        ReplacesDocumentId = replacesDocumentId;
    }

    public Guid PersonnelId { get; private set; }
    public Personnel Personnel { get; private set; } = null!;
    public Guid? AssignmentHistoryId { get; private set; }
    public AssignmentHistory? AssignmentHistory { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; }
    public string? Description { get; private set; }
    public Guid? ReplacesDocumentId { get; private set; }
    public AssignmentDocument? ReplacesDocument { get; private set; }

    public void SoftDelete(DateTimeOffset deletedAt)
    {
        IsDeleted = true;
        DeletedAt = deletedAt;
        UpdatedAt = deletedAt;
    }
}
