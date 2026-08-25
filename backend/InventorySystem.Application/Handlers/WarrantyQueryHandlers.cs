using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Queries;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class WarrantyQueryHandlers(
    IDeviceRepository deviceRepository,
    IWarrantyStatusService warrantyStatusService)
    : IRequestHandler<GetExpiringWarrantiesQuery, IReadOnlyList<WarrantyAlertDeviceDto>>,
      IRequestHandler<GetExpiredWarrantiesQuery, IReadOnlyList<WarrantyAlertDeviceDto>>,
      IRequestHandler<GetDevicesWithoutWarrantyQuery, IReadOnlyList<WarrantyAlertDeviceDto>>,
      IRequestHandler<GetWarrantySummaryQuery, WarrantySummaryDto>
{
    public Task<IReadOnlyList<WarrantyAlertDeviceDto>> Handle(GetExpiringWarrantiesQuery request, CancellationToken cancellationToken) =>
        GetByStatus(WarrantyStatus.ExpiringSoon, cancellationToken);

    public Task<IReadOnlyList<WarrantyAlertDeviceDto>> Handle(GetExpiredWarrantiesQuery request, CancellationToken cancellationToken) =>
        GetByStatus(WarrantyStatus.Expired, cancellationToken);

    public Task<IReadOnlyList<WarrantyAlertDeviceDto>> Handle(GetDevicesWithoutWarrantyQuery request, CancellationToken cancellationToken) =>
        GetByStatus(WarrantyStatus.NoWarranty, cancellationToken);

    public async Task<WarrantySummaryDto> Handle(GetWarrantySummaryQuery request, CancellationToken cancellationToken)
    {
        var devices = await deviceRepository.ListAsync(cancellationToken);
        var statuses = devices.Select(device => warrantyStatusService.Calculate(device).Status).ToList();
        return new WarrantySummaryDto(
            warrantyStatusService.ExpiringSoonDays,
            statuses.Count(status => status == WarrantyStatus.ExpiringSoon),
            statuses.Count(status => status == WarrantyStatus.Expired),
            statuses.Count(status => status == WarrantyStatus.NoWarranty));
    }

    private async Task<IReadOnlyList<WarrantyAlertDeviceDto>> GetByStatus(
        WarrantyStatus requestedStatus,
        CancellationToken cancellationToken)
    {
        var devices = await deviceRepository.ListAsync(cancellationToken);
        return devices
            .Select(device => new { Device = device, Warranty = warrantyStatusService.Calculate(device) })
            .Where(item => item.Warranty.Status == requestedStatus)
            .OrderBy(item => item.Device.WarrantyEndDate)
            .ThenBy(item => item.Device.CihazAdi)
            .Select(item => Map(item.Device, item.Warranty))
            .ToList();
    }

    private static WarrantyAlertDeviceDto Map(Device device, WarrantyStatusResult warranty) => new(
        device.Id,
        device.CihazAdi,
        device.SeriNo,
        device.EnvanterNo,
        device.WarrantyEndDate,
        warranty.Status,
        warranty.StatusName,
        warranty.RemainingDays,
        warranty.Message);
}
