using InventorySystem.Application.Commands;
using InventorySystem.Application.Common.Audit;
using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class DeviceCommandHandlers(
    IDeviceRepository deviceRepository,
    IDateTimeProvider dateTimeProvider,
    IAuditLogService auditLogService)
    : IRequestHandler<CreateDeviceCommand, Guid>,
      IRequestHandler<UpdateDeviceCommand>,
      IRequestHandler<DeleteDeviceCommand>
{
    public async Task<Guid> Handle(
        CreateDeviceCommand request,
        CancellationToken cancellationToken)
    {
        if (request.PersonelId.HasValue || request.Status == DeviceStatus.Assigned)
        {
            throw new BusinessRuleException(
                "Cihaz zimmeti cihaz kayıt ekranından verilemez. Zimmet işlemini kullanın.");
        }

        var device = new Device(
            request.CihazAdi,
            request.SeriNo,
            request.EnvanterNo,
            request.BarkodNo,
            request.Marka,
            request.Model,
            request.CategoryId,
            request.Status,
            request.PersonelId,
            request.WarrantyStartDate,
            request.WarrantyEndDate,
            request.WarrantyProvider,
            request.WarrantyNote);

        await deviceRepository.AddAsync(device, cancellationToken);
        await auditLogService.RecordAsync(
            AuditActionTypes.DeviceCreated, AuditEntityTypes.Device, device.Id,
            $"{device.CihazAdi} cihazı eklendi.", null, AuditSnapshots.Device(device),
            !device.PersonelId.HasValue, cancellationToken);
        await deviceRepository.SaveChangesAsync(cancellationToken);

        return device.Id;
    }

    public async Task Handle(
        UpdateDeviceCommand request,
        CancellationToken cancellationToken)
    {
        var device = await deviceRepository.GetByIdAsync(request.Id, cancellationToken);

        if (device == null)
        {
            throw new NotFoundException("Cihaz bulunamadı.");
        }

        if (request.PersonelId != device.PersonelId)
        {
            throw new BusinessRuleException(
                "Zimmetli personel cihaz düzenleme ekranından değiştirilemez. Zimmet/iade işlemini kullanın.");
        }

        if (device.PersonelId.HasValue && request.Status != DeviceStatus.Assigned)
        {
            throw new BusinessRuleException(
                "Zimmetli cihazın durumu doğrudan değiştirilemez. Önce zimmeti iade alın.");
        }

        if (!device.PersonelId.HasValue && request.Status == DeviceStatus.Assigned)
        {
            throw new BusinessRuleException(
                "Cihaz durumu doğrudan zimmetli yapılamaz. Zimmet işlemini kullanın.");
        }

        var oldValues = AuditSnapshots.Device(device);
        device.Update(
            request.CihazAdi,
            request.SeriNo,
            request.EnvanterNo,
            request.BarkodNo,
            request.Marka,
            request.Model,
            request.CategoryId,
            request.Status,
            request.PersonelId,
            dateTimeProvider.UtcNow,
            request.WarrantyStartDate,
            request.WarrantyEndDate,
            request.WarrantyProvider,
            request.WarrantyNote);

        var newValues = AuditSnapshots.Device(device);
        var warrantyChanged = oldValues.WarrantyStartDate != newValues.WarrantyStartDate
            || oldValues.WarrantyEndDate != newValues.WarrantyEndDate
            || oldValues.WarrantyProvider != newValues.WarrantyProvider
            || oldValues.WarrantyNote != newValues.WarrantyNote;
        var canUndo = oldValues.PersonelId == newValues.PersonelId;
        await auditLogService.RecordAsync(
            AuditActionTypes.DeviceUpdated, AuditEntityTypes.Device, device.Id,
            $"{device.CihazAdi} cihazı güncellendi.",
            oldValues, newValues, canUndo, cancellationToken);
        if (oldValues.Status != (int)DeviceStatus.Scrap && device.Status == DeviceStatus.Scrap)
        {
            await auditLogService.RecordAsync(
                AuditActionTypes.DeviceScrapped, AuditEntityTypes.Device, device.Id,
                $"{device.CihazAdi} cihazı hurda / imha durumuna alındı.",
                new { Status = oldValues.Status }, new { Status = newValues.Status },
                false, cancellationToken);
        }
        if (warrantyChanged)
        {
            await auditLogService.RecordAsync(
                AuditActionTypes.WarrantyUpdated, AuditEntityTypes.Device, device.Id,
                $"{device.CihazAdi} cihazının garanti bilgileri güncellendi.",
                new { oldValues.WarrantyStartDate, oldValues.WarrantyEndDate, oldValues.WarrantyProvider, oldValues.WarrantyNote },
                new { newValues.WarrantyStartDate, newValues.WarrantyEndDate, newValues.WarrantyProvider, newValues.WarrantyNote },
                false, cancellationToken);
        }
        await deviceRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(
        DeleteDeviceCommand request,
        CancellationToken cancellationToken)
    {
        var device = await deviceRepository.GetByIdAsync(request.Id, cancellationToken);

        if (device == null)
        {
            throw new NotFoundException("Cihaz bulunamadı.");
        }

        if (device.PersonelId.HasValue)
        {
            throw new BusinessRuleException("Zimmetli cihaz silinemez. Önce zimmeti iade alın.");
        }

        var oldValues = AuditSnapshots.Device(device);
        device.SoftDelete(dateTimeProvider.UtcNow);
        await auditLogService.RecordAsync(
            AuditActionTypes.DeviceDeleted, AuditEntityTypes.Device, device.Id,
            $"{device.CihazAdi} cihazı silindi.", oldValues, AuditSnapshots.Device(device),
            true, cancellationToken);

        await deviceRepository.SaveChangesAsync(cancellationToken);
    }
}
