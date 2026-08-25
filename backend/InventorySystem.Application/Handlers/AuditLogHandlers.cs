using System.Text.Json;
using InventorySystem.Application.Commands;
using InventorySystem.Application.Common.Audit;
using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Queries;
using InventorySystem.Domain.Enums;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class GetAuditLogsQueryHandler(IAuditLogRepository repository, IIdentityService identityService)
    : IRequestHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>
{
    public async Task<IReadOnlyList<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await repository.ListAsync(
            request.ActionType, request.UserId, request.SystemUserOnly, request.EntityType,
            request.From, request.To, cancellationToken);
        var displayNames = await identityService.GetDisplayNamesAsync(
            logs.Where(log => log.UserId.HasValue).Select(log => log.UserId!.Value), cancellationToken);
        return logs.Select(log => new AuditLogDto(
            log.Id, log.UserId, log.UserId.HasValue && displayNames.TryGetValue(log.UserId.Value, out var displayName)
                ? displayName : log.UserId.HasValue ? "Silinmiş kullanıcı" : "Sistem",
            log.ActionType, log.EntityType, log.EntityId, log.Description,
            log.OldValues, log.NewValues, log.CreatedAt,
            log.CanUndo && !log.UndoneAt.HasValue, log.UndoneAt, log.UndoAuditLogId, log.OperationId)).ToList();
    }
}

public sealed class UndoAuditLogCommandHandler(
    IAuditLogRepository repository,
    IAuditLogService auditLogService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<UndoAuditLogCommand, UndoAuditLogResultDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<UndoAuditLogResultDto> Handle(UndoAuditLogCommand request, CancellationToken cancellationToken)
    {
        var log = await repository.GetByIdAsync(request.AuditLogId, cancellationToken)
            ?? throw new NotFoundException("Audit kaydı bulunamadı.");
        if (!log.CanUndo || log.UndoneAt.HasValue)
            throw new BusinessRuleException("Bu işlem geri alınamaz veya daha önce geri alınmış.");

        await ApplyUndoAsync(log, cancellationToken);
        var now = dateTimeProvider.UtcNow;
        var undoLog = await auditLogService.RecordAsync(
            AuditActionTypes.Undo, log.EntityType, log.EntityId,
            $"'{log.Description}' işlemi geri alındı.", ParseJson(log.NewValues), ParseJson(log.OldValues),
            false, cancellationToken, operationId: log.OperationId);
        log.MarkUndone(now, undoLog.Id);
        await repository.SaveChangesAsync(cancellationToken);
        return new UndoAuditLogResultDto(log.Id, undoLog.Id, now);
    }

    private async Task ApplyUndoAsync(Domain.Entities.AuditLog log, CancellationToken cancellationToken)
    {
        switch (log.ActionType)
        {
            case AuditActionTypes.PersonnelCreated:
            case AuditActionTypes.PersonnelUpdated:
            case AuditActionTypes.PersonnelDeleted:
                await UndoPersonnelAsync(log, cancellationToken);
                return;
            case AuditActionTypes.DeviceCreated:
            case AuditActionTypes.DeviceUpdated:
            case AuditActionTypes.WarrantyUpdated:
            case AuditActionTypes.DeviceScrapped:
            case AuditActionTypes.DeviceDeleted:
                await UndoDeviceAsync(log, cancellationToken);
                return;
            case AuditActionTypes.DeviceAssigned:
                await UndoAssignmentAsync(log, cancellationToken);
                return;
            default:
                throw new BusinessRuleException("Bu işlem türü için güvenli geri alma desteği yok.");
        }
    }

    private async Task UndoPersonnelAsync(Domain.Entities.AuditLog log, CancellationToken cancellationToken)
    {
        var entity = await repository.GetPersonnelIncludingDeletedAsync(log.EntityId, cancellationToken)
            ?? throw new ConflictException("Personel artık mevcut değil; işlem geri alınamadı.");
        var expected = Deserialize<PersonnelAuditSnapshot>(log.NewValues);
        if (AuditSnapshots.Personnel(entity) != expected)
            throw new ConflictException("Personel kaydı bu işlemden sonra değişmiş. Güncel verinin üzerine yazılmadı.");

        if (log.ActionType == AuditActionTypes.PersonnelCreated)
        {
            if (await repository.HasActiveDevicesAsync(entity.Id, cancellationToken))
                throw new ConflictException("Personele bağlı cihaz bulunduğu için ekleme işlemi geri alınamaz.");
            entity.SoftDelete(dateTimeProvider.UtcNow);
            return;
        }

        var old = Deserialize<PersonnelAuditSnapshot>(log.OldValues);
        if (log.ActionType == AuditActionTypes.PersonnelDeleted)
        {
            if (await repository.SicilNoExistsAsync(old.SicilNo, entity.Id, cancellationToken))
                throw new ConflictException("Sicil numarası başka bir aktif kayıtta kullanılıyor.");
            entity.RestoreFromDelete(old.AktifMi, dateTimeProvider.UtcNow);
            return;
        }

        if (await repository.SicilNoExistsAsync(old.SicilNo, entity.Id, cancellationToken))
            throw new ConflictException("Eski sicil numarası başka bir aktif kayıtta kullanılıyor.");
        entity.Update(old.SicilNo, old.Ad, old.Soyad, old.Departman, old.Pozisyon,
            old.ZimmetNo, old.AktifMi, dateTimeProvider.UtcNow);
    }

    private async Task UndoDeviceAsync(Domain.Entities.AuditLog log, CancellationToken cancellationToken)
    {
        var entity = await repository.GetDeviceIncludingDeletedAsync(log.EntityId, cancellationToken)
            ?? throw new ConflictException("Cihaz artık mevcut değil; işlem geri alınamadı.");
        var expected = Deserialize<DeviceAuditSnapshot>(log.NewValues);
        if (AuditSnapshots.Device(entity) != expected)
            throw new ConflictException("Cihaz kaydı bu işlemden sonra değişmiş. Güncel verinin üzerine yazılmadı.");

        if (log.ActionType == AuditActionTypes.DeviceCreated)
        {
            if (entity.PersonelId.HasValue)
                throw new ConflictException("Cihaz zimmetli olduğu için ekleme işlemi geri alınamaz.");
            entity.SoftDelete(dateTimeProvider.UtcNow);
            return;
        }

        var old = Deserialize<DeviceAuditSnapshot>(log.OldValues);
        if (await repository.SerialOrInventoryExistsAsync(old.SeriNo, old.EnvanterNo, entity.Id, cancellationToken))
            throw new ConflictException("Eski seri veya envanter numarası başka bir aktif cihazda kullanılıyor.");

        if (log.ActionType == AuditActionTypes.DeviceDeleted)
        {
            entity.RestoreFromDelete(dateTimeProvider.UtcNow);
            return;
        }

        entity.Update(old.CihazAdi, old.SeriNo, old.EnvanterNo, old.BarkodNo,
            old.Marka, old.Model, old.CategoryId, (DeviceStatus)old.Status, old.PersonelId,
            dateTimeProvider.UtcNow, old.WarrantyStartDate, old.WarrantyEndDate,
            old.WarrantyProvider, old.WarrantyNote);
    }

    private async Task UndoAssignmentAsync(Domain.Entities.AuditLog log, CancellationToken cancellationToken)
    {
        var snapshot = Deserialize<AssignmentAuditSnapshot>(log.NewValues);
        var assignment = await repository.GetAssignmentAsync(snapshot.AssignmentHistoryId, cancellationToken)
            ?? throw new ConflictException("Zimmet geçmişi bulunamadı.");
        var device = await repository.GetDeviceIncludingDeletedAsync(snapshot.DeviceId, cancellationToken)
            ?? throw new ConflictException("Zimmetli cihaz bulunamadı.");
        if (assignment.ReturnedAt.HasValue || assignment.IsDeleted ||
            device.IsDeleted || device.PersonelId != snapshot.PersonnelId || device.Status != DeviceStatus.Assigned)
            throw new ConflictException("Zimmet durumu sonradan değişmiş. İşlem geri alınmadı.");
        assignment.UndoAssignment(dateTimeProvider.UtcNow);
        device.Unassign(dateTimeProvider.UtcNow);
    }

    private static T Deserialize<T>(string? json) where T : class =>
        string.IsNullOrWhiteSpace(json)
            ? throw new BusinessRuleException("Geri alma verisi eksik.")
            : JsonSerializer.Deserialize<T>(json, JsonOptions)
              ?? throw new BusinessRuleException("Geri alma verisi okunamadı.");

    private static object? ParseJson(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
}
