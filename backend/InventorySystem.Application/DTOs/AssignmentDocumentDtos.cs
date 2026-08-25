namespace InventorySystem.Application.DTOs;

public sealed record AssignmentDocumentDto(
    Guid Id,
    Guid PersonnelId,
    Guid? AssignmentHistoryId,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    DateTimeOffset UploadedAt,
    string? Description,
    Guid? ReplacesDocumentId);

public sealed record GeneratedAssignmentDocumentDto(
    string FileName,
    string ContentType,
    byte[] Content);

public sealed record StoredAssignmentDocumentFileDto(
    string FileName,
    string ContentType,
    byte[] Content);

public sealed record AssignmentDocumentPdfModel(
    string PersonnelName,
    string SicilNo,
    string Department,
    string? AssignmentNumber,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<AssignmentDocumentDeviceModel> Devices);

public sealed record AssignmentDocumentDeviceModel(
    string DeviceName,
    string BrandModel,
    string SerialNumber,
    string InventoryNumber,
    DateTimeOffset AssignedAt);
