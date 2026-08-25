using InventorySystem.Application.DTOs;
using MediatR;

namespace InventorySystem.Application.Queries;

public sealed record GenerateAssignmentDocumentQuery(Guid PersonnelId)
    : IRequest<GeneratedAssignmentDocumentDto>;

public sealed record GetAssignmentDocumentsQuery(Guid PersonnelId)
    : IRequest<IReadOnlyList<AssignmentDocumentDto>>;

public sealed record GetAssignmentDocumentFileQuery(Guid PersonnelId, Guid DocumentId)
    : IRequest<StoredAssignmentDocumentFileDto>;
