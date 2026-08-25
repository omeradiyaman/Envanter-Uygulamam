using InventorySystem.Application.DTOs;
using MediatR;

namespace InventorySystem.Application.Commands;

public sealed record UploadAssignmentDocumentCommand(
    Guid PersonnelId,
    Guid? AssignmentHistoryId,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    byte[] Content,
    string? Description,
    Guid? ReplacesDocumentId) : IRequest<AssignmentDocumentDto>;

public sealed record DeleteAssignmentDocumentCommand(Guid PersonnelId, Guid DocumentId) : IRequest;
