using InventorySystem.Application.Commands;
using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.Common.Audit;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Queries;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class GenerateAssignmentDocumentQueryHandler(
    IPersonnelRepository personnelRepository,
    IAssignmentRepository assignmentRepository,
    IAssignmentDocumentPdfGenerator pdfGenerator,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GenerateAssignmentDocumentQuery, GeneratedAssignmentDocumentDto>
{
    public async Task<GeneratedAssignmentDocumentDto> Handle(
        GenerateAssignmentDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var personnel = await personnelRepository.GetByIdAsync(request.PersonnelId, cancellationToken)
            ?? throw new NotFoundException("Personel bulunamadı.");
        var history = await assignmentRepository.GetPersonnelHistoryAsync(request.PersonnelId, cancellationToken);
        var activeHistory = history.Where(item => item.IsActive)
            .ToDictionary(item => item.DeviceId);

        var devices = personnel.Devices
            .Where(device => device.PersonelId == personnel.Id && device.Status == DeviceStatus.Assigned)
            .OrderBy(device => device.CihazAdi)
            .Select(device => new AssignmentDocumentDeviceModel(
                device.CihazAdi,
                $"{device.Marka} / {device.Model}",
                device.SeriNo,
                device.EnvanterNo,
                activeHistory.TryGetValue(device.Id, out var assignment)
                    ? assignment.AssignedAt
                    : device.UpdatedAt ?? device.CreatedAt))
            .ToList();

        var generatedAt = dateTimeProvider.UtcNow;
        var model = new AssignmentDocumentPdfModel(
            $"{personnel.Ad} {personnel.Soyad}",
            personnel.SicilNo,
            personnel.Departman,
            personnel.ZimmetNo,
            generatedAt,
            devices);

        var safeRegistration = string.Concat(personnel.SicilNo.Where(char.IsLetterOrDigit));
        var fileName = $"zimmet-belgesi-{safeRegistration}-{generatedAt:yyyyMMdd-HHmm}.pdf";
        return new GeneratedAssignmentDocumentDto(fileName, "application/pdf", pdfGenerator.Generate(model));
    }
}

public sealed class GetAssignmentDocumentsQueryHandler(IAssignmentDocumentRepository repository)
    : IRequestHandler<GetAssignmentDocumentsQuery, IReadOnlyList<AssignmentDocumentDto>>
{
    public async Task<IReadOnlyList<AssignmentDocumentDto>> Handle(
        GetAssignmentDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var documents = await repository.ListByPersonnelAsync(request.PersonnelId, cancellationToken);
        return documents.Select(Map).ToList();
    }

    internal static AssignmentDocumentDto Map(AssignmentDocument document) => new(
        document.Id,
        document.PersonnelId,
        document.AssignmentHistoryId,
        document.OriginalFileName,
        document.ContentType,
        document.FileSize,
        document.UploadedAt,
        document.Description,
        document.ReplacesDocumentId);
}

public sealed class GetAssignmentDocumentFileQueryHandler(
    IAssignmentDocumentRepository repository,
    IAssignmentDocumentStorage storage)
    : IRequestHandler<GetAssignmentDocumentFileQuery, StoredAssignmentDocumentFileDto>
{
    public async Task<StoredAssignmentDocumentFileDto> Handle(
        GetAssignmentDocumentFileQuery request,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null || document.PersonnelId != request.PersonnelId)
        {
            throw new NotFoundException("Belge bulunamadı.");
        }

        var content = await storage.ReadAsync(document.StoredFileName, cancellationToken);
        return new StoredAssignmentDocumentFileDto(document.OriginalFileName, document.ContentType, content);
    }
}

public sealed class UploadAssignmentDocumentCommandHandler(
    IPersonnelRepository personnelRepository,
    IAssignmentRepository assignmentRepository,
    IAssignmentDocumentRepository documentRepository,
    IAssignmentDocumentStorage storage,
    IDateTimeProvider dateTimeProvider,
    IAuditLogService auditLogService)
    : IRequestHandler<UploadAssignmentDocumentCommand, AssignmentDocumentDto>
{
    public async Task<AssignmentDocumentDto> Handle(
        UploadAssignmentDocumentCommand request,
        CancellationToken cancellationToken)
    {
        _ = await personnelRepository.GetByIdAsync(request.PersonnelId, cancellationToken)
            ?? throw new NotFoundException("Personel bulunamadı.");

        if (request.AssignmentHistoryId.HasValue)
        {
            var history = await assignmentRepository.GetPersonnelHistoryAsync(request.PersonnelId, cancellationToken);
            if (!history.Any(item => item.Id == request.AssignmentHistoryId.Value))
            {
                throw new NotFoundException("Belirtilen zimmet kaydı bu personele ait değil.");
            }
        }

        if (request.ReplacesDocumentId.HasValue)
        {
            var previous = await documentRepository.GetByIdAsync(request.ReplacesDocumentId.Value, cancellationToken);
            if (previous is null || previous.PersonnelId != request.PersonnelId)
            {
                throw new NotFoundException("Sürümlenecek belge bulunamadı.");
            }
        }

        ValidateFileSignature(request.ContentType, request.Content);
        var extension = request.ContentType.ToLowerInvariant() switch
        {
            "application/pdf" => ".pdf",
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            _ => throw new InvalidOperationException("Desteklenmeyen dosya türü.")
        };
        var storedFileName = await storage.SaveAsync(request.Content, extension, cancellationToken);
        var originalFileName = Path.GetFileName(request.OriginalFileName.Replace('\\', '/'));
        originalFileName = new string(originalFileName.Where(character => !char.IsControl(character)).ToArray());
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw CreateFileValidationException("Dosya adı geçersiz.");
        }
        var document = new AssignmentDocument(
            request.PersonnelId,
            request.AssignmentHistoryId,
            originalFileName,
            storedFileName,
            request.ContentType.ToLowerInvariant(),
            request.FileSize,
            dateTimeProvider.UtcNow,
            string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            request.ReplacesDocumentId);

        await documentRepository.AddAsync(document, cancellationToken);
        await auditLogService.RecordAsync(
            AuditActionTypes.DocumentUploaded, AuditEntityTypes.AssignmentDocument, document.Id,
            $"{document.OriginalFileName} imzalı zimmet belgesi yüklendi.", null,
            new { document.PersonnelId, document.AssignmentHistoryId, document.OriginalFileName,
                document.ContentType, document.FileSize, document.UploadedAt, document.Description },
            false, cancellationToken);
        await documentRepository.SaveChangesAsync(cancellationToken);
        return GetAssignmentDocumentsQueryHandler.Map(document);
    }

    private static void ValidateFileSignature(string contentType, byte[] content)
    {
        var valid = contentType.ToLowerInvariant() switch
        {
            "application/pdf" => content.AsSpan().StartsWith("%PDF-"u8),
            "image/png" => content.AsSpan().StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            "image/jpeg" => content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF,
            _ => false
        };

        if (!valid)
        {
            throw CreateFileValidationException("Dosya içeriği bildirilen dosya türüyle eşleşmiyor.");
        }
    }

    private static FluentValidation.ValidationException CreateFileValidationException(string message) =>
        new([new FluentValidation.Results.ValidationFailure("File", message)]);
}

public sealed class DeleteAssignmentDocumentCommandHandler(
    IAssignmentDocumentRepository repository,
    IDateTimeProvider dateTimeProvider,
    IAuditLogService auditLogService)
    : IRequestHandler<DeleteAssignmentDocumentCommand>
{
    public async Task Handle(DeleteAssignmentDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null || document.PersonnelId != request.PersonnelId)
        {
            throw new NotFoundException("Belge bulunamadı.");
        }

        var oldValues = new { document.PersonnelId, document.AssignmentHistoryId,
            document.OriginalFileName, document.ContentType, document.FileSize,
            document.UploadedAt, document.Description, document.IsDeleted };
        document.SoftDelete(dateTimeProvider.UtcNow);
        await auditLogService.RecordAsync(
            AuditActionTypes.DocumentDeleted, AuditEntityTypes.AssignmentDocument, document.Id,
            $"{document.OriginalFileName} imzalı zimmet belgesi silindi.", oldValues,
            new { document.PersonnelId, document.OriginalFileName, document.IsDeleted, document.DeletedAt },
            false, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
