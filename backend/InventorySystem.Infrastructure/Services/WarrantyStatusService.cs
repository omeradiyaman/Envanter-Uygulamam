using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace InventorySystem.Infrastructure.Services;

public sealed class WarrantyStatusService(
    IConfiguration configuration,
    IDateTimeProvider dateTimeProvider) : IWarrantyStatusService
{
    public int ExpiringSoonDays { get; } = Math.Max(
        1,
        int.TryParse(configuration["WarrantySettings:ExpiringSoonDays"], out var configuredDays)
            ? configuredDays
            : 90);

    public WarrantyStatusResult Calculate(Device device)
    {
        if (!device.WarrantyEndDate.HasValue)
        {
            return new WarrantyStatusResult(WarrantyStatus.NoWarranty, "Garanti bilgisi yok", null, "Garanti bitiş tarihi girilmemiş.");
        }

        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);
        var remainingDays = device.WarrantyEndDate.Value.DayNumber - today.DayNumber;
        if (remainingDays < 0)
        {
            var elapsedDays = Math.Abs(remainingDays);
            return new WarrantyStatusResult(WarrantyStatus.Expired, "Süresi dolmuş", remainingDays, $"Garanti {elapsedDays} gün önce sona erdi.");
        }

        if (remainingDays <= ExpiringSoonDays)
        {
            return new WarrantyStatusResult(WarrantyStatus.ExpiringSoon, "Yakında bitecek", remainingDays, remainingDays == 0 ? "Garanti bugün sona eriyor." : $"Garanti bitimine {remainingDays} gün kaldı.");
        }

        return new WarrantyStatusResult(WarrantyStatus.Active, "Aktif", remainingDays, $"Garanti bitimine {remainingDays} gün kaldı.");
    }
}
