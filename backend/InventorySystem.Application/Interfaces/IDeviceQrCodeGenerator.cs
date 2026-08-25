using InventorySystem.Application.DTOs;

namespace InventorySystem.Application.Interfaces;

public interface IDeviceQrCodeGenerator
{
    DeviceQrCodeDto Generate(Guid deviceId);
}
