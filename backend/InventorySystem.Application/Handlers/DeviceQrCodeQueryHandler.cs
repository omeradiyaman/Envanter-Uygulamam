using InventorySystem.Application.DTOs;
using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Queries;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class DeviceQrCodeQueryHandler(
    IDeviceRepository deviceRepository,
    IDeviceQrCodeGenerator qrCodeGenerator)
    : IRequestHandler<GetDeviceQrCodeQuery, DeviceQrCodeDto>
{
    public async Task<DeviceQrCodeDto> Handle(
        GetDeviceQrCodeQuery request,
        CancellationToken cancellationToken)
    {
        var device = await deviceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (device is null)
        {
            throw new NotFoundException("Cihaz bulunamadı.");
        }

        return qrCodeGenerator.Generate(device.Id);
    }
}
