using InventorySystem.Application.DTOs;
using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Queries;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class DeviceQueryHandlers(
    IDeviceRepository deviceRepository,
    IWarrantyStatusService warrantyStatusService)
    : IRequestHandler<GetDeviceListQuery, IReadOnlyList<DeviceListDto>>,
      IRequestHandler<GetDeviceByIdQuery, DeviceDto>,
      IRequestHandler<GetDeviceCategoriesQuery, IReadOnlyList<DeviceCategoryDto>>
{
    public async Task<IReadOnlyList<DeviceListDto>> Handle(
        GetDeviceListQuery request,
        CancellationToken cancellationToken)
    {
        var devices = await deviceRepository.ListAsync(cancellationToken);

        return devices.Select(MapListItem).ToList();
    }

    public async Task<DeviceDto> Handle(
        GetDeviceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var device = await deviceRepository.GetByIdAsync(request.Id, cancellationToken);

        if (device == null)
        {
            throw new NotFoundException("Cihaz bulunamadı.");
        }

        var warranty = warrantyStatusService.Calculate(device);
        return new DeviceDto(
            device.Id,
            device.CihazAdi,
            device.SeriNo,
            device.EnvanterNo,
            device.BarkodNo,
            device.Marka,
            device.Model,
            device.CategoryId,
            device.Category.Name,
            device.Status,
            device.Status.ToString(), // Or map to a friendly name
            device.PersonelId,
            device.Personel != null ? $"{device.Personel.Ad} {device.Personel.Soyad}" : null,
            device.CreatedAt,
            device.UpdatedAt,
            device.WarrantyStartDate,
            device.WarrantyEndDate,
            device.WarrantyProvider,
            device.WarrantyNote,
            warranty.Status,
            warranty.StatusName,
            warranty.RemainingDays,
            warranty.Message
        );
    }

    public async Task<IReadOnlyList<DeviceCategoryDto>> Handle(
        GetDeviceCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await deviceRepository.ListCategoriesAsync(cancellationToken);

        var result = new List<DeviceCategoryDto>(categories.Count);
        foreach (var category in categories)
        {
            result.Add(new DeviceCategoryDto(
                category.Id,
                category.Name,
                category.Description,
                await deviceRepository.CategoryDeviceCountAsync(category.Id, cancellationToken)));
        }
        return result;
    }

    private DeviceListDto MapListItem(InventorySystem.Domain.Entities.Device device)
    {
        var warranty = warrantyStatusService.Calculate(device);
        return new DeviceListDto(
            device.Id,
            device.CihazAdi,
            device.SeriNo,
            device.EnvanterNo,
            device.Marka,
            device.Model,
            device.Category.Name,
            device.Status,
            device.Status.ToString(),
            device.Personel != null ? $"{device.Personel.Ad} {device.Personel.Soyad}" : null,
            device.CreatedAt,
            device.WarrantyStartDate,
            device.WarrantyEndDate,
            device.WarrantyProvider,
            device.WarrantyNote,
            warranty.Status,
            warranty.StatusName,
            warranty.RemainingDays,
            warranty.Message);
    }
}
