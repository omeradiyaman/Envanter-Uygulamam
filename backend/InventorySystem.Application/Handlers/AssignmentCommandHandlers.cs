using InventorySystem.Application.Commands;
using InventorySystem.Application.Common.Audit;
using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class AssignDeviceCommandHandler(
    IDeviceRepository deviceRepository,
    IPersonnelRepository personnelRepository,
    IAssignmentRepository assignmentRepository,
    IDateTimeProvider dateTimeProvider,
    IAuditLogService auditLogService)
    : IRequestHandler<AssignDeviceCommand>
{
    public async Task Handle(AssignDeviceCommand request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;

        // Load device (ignoring soft-delete filter via IgnoreQueryFilters not needed here,
        // global filter only hides IsDeleted records, so not found means deleted or missing)
        var device = await deviceRepository.GetByIdAsync(request.DeviceId, cancellationToken)
            ?? throw new NotFoundException("Cihaz bulunamadı.");

        var personnel = await personnelRepository.GetByIdAsync(request.PersonnelId, cancellationToken)
            ?? throw new NotFoundException("Personel bulunamadı.");

        // Business rules
        if (!personnel.AktifMi)
            throw new BusinessRuleException("Pasif personele cihaz zimmetlenemez.");

        var nonAssignableStatuses = new[] { DeviceStatus.Scrap, DeviceStatus.Lost, DeviceStatus.Passive };
        if (nonAssignableStatuses.Contains(device.Status))
            throw new BusinessRuleException($"'{device.Status}' durumundaki cihaz zimmetlenemez.");

        if (device.PersonelId.HasValue)
            throw new BusinessRuleException(
                "Bu cihaz başka bir personele zimmetlidir. Önce mevcut zimmeti iade alın.");

        // Check for orphaned active history (defensive check)
        var existingActive = await assignmentRepository.GetActiveAssignmentAsync(
            request.DeviceId, cancellationToken);

        if (existingActive != null)
            throw new BusinessRuleException(
                "Bu cihaz için aktif bir zimmet kaydı bulunmaktadır. Önce zimmeti iade alın.");

        // Apply domain changes
        device.Assign(request.PersonnelId, now);

        var history = new AssignmentHistory(
            request.DeviceId,
            request.PersonnelId,
            now,
            request.Note);

        await assignmentRepository.AddAsync(history, cancellationToken);
        await auditLogService.RecordAsync(
            AuditActionTypes.DeviceAssigned, AuditEntityTypes.Assignment, history.Id,
            $"{device.CihazAdi} cihazı {personnel.Ad} {personnel.Soyad} personeline zimmetlendi.",
            null, AuditSnapshots.Assignment(history), true, cancellationToken);

        // Save both device update and history insert in a single transaction
        await assignmentRepository.SaveChangesAsync(cancellationToken);
    }
}

public sealed class UnassignDeviceCommandHandler(
    IDeviceRepository deviceRepository,
    IAssignmentRepository assignmentRepository,
    IDateTimeProvider dateTimeProvider,
    IAuditLogService auditLogService)
    : IRequestHandler<UnassignDeviceCommand>
{
    public async Task Handle(UnassignDeviceCommand request, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;

        var device = await deviceRepository.GetByIdAsync(request.DeviceId, cancellationToken)
            ?? throw new NotFoundException("Cihaz bulunamadı.");

        if (!device.PersonelId.HasValue)
            throw new BusinessRuleException("Bu cihaz herhangi bir personele zimmetli değil.");

        var activeAssignment = await assignmentRepository.GetActiveAssignmentAsync(
            request.DeviceId, cancellationToken);

        if (activeAssignment == null)
            throw new BusinessRuleException("Bu cihaz için aktif zimmet kaydı bulunamadı.");

        // Apply domain changes
        var oldValues = AuditSnapshots.Assignment(activeAssignment);
        activeAssignment.MarkReturned(now, note: request.Note);
        device.Unassign(now);

        await auditLogService.RecordAsync(
            AuditActionTypes.DeviceReturned, AuditEntityTypes.Assignment, activeAssignment.Id,
            $"{device.CihazAdi} cihazının zimmeti iade alındı.", oldValues,
            AuditSnapshots.Assignment(activeAssignment), false, cancellationToken);

        await assignmentRepository.SaveChangesAsync(cancellationToken);
    }
}
