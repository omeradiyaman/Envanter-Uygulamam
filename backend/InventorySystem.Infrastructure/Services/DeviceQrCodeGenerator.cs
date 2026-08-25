using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using QRCoder;

namespace InventorySystem.Infrastructure.Services;

public sealed class DeviceQrCodeGenerator(IConfiguration configuration) : IDeviceQrCodeGenerator
{
    public DeviceQrCodeDto Generate(Guid deviceId)
    {
        var publicBaseUrl = configuration["DeviceQr:PublicBaseUrl"];
        if (string.IsNullOrWhiteSpace(publicBaseUrl))
        {
            throw new InvalidOperationException("DeviceQr:PublicBaseUrl yapılandırılmalıdır.");
        }

        var deviceUrl = $"{publicBaseUrl.TrimEnd('/')}/devices/{deviceId}";
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(deviceUrl, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(qrData).GetGraphic(12);

        return new DeviceQrCodeDto(deviceId, deviceUrl, Convert.ToBase64String(png));
    }
}
