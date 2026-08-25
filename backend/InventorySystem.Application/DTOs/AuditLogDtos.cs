namespace InventorySystem.Application.DTOs;

public sealed record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string UserDisplayName,
    string ActionType,
    string EntityType,
    Guid EntityId,
    string Description,
    string? OldValues,
    string? NewValues,
    DateTimeOffset CreatedAt,
    bool CanUndo,
    DateTimeOffset? UndoneAt,
    Guid? UndoAuditLogId,
    Guid? OperationId);

public sealed record UndoAuditLogResultDto(Guid AuditLogId, Guid UndoAuditLogId, DateTimeOffset UndoneAt);

public sealed record PersonnelAuditSnapshot(
    string SicilNo,
    string Ad,
    string Soyad,
    string Departman,
    string Pozisyon,
    string? ZimmetNo,
    bool AktifMi,
    bool IsDeleted);

public sealed record DeviceAuditSnapshot(
    string CihazAdi,
    string SeriNo,
    string EnvanterNo,
    string? BarkodNo,
    string Marka,
    string Model,
    Guid CategoryId,
    int Status,
    Guid? PersonelId,
    DateOnly? WarrantyStartDate,
    DateOnly? WarrantyEndDate,
    string? WarrantyProvider,
    string? WarrantyNote,
    bool IsDeleted);

public sealed record AssignmentAuditSnapshot(
    Guid AssignmentHistoryId,
    Guid DeviceId,
    Guid PersonnelId,
    DateTimeOffset AssignedAt,
    DateTimeOffset? ReturnedAt);
