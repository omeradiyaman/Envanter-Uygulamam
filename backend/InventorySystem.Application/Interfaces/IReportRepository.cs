using InventorySystem.Application.DTOs;
using InventorySystem.Domain.Enums;

namespace InventorySystem.Application.Interfaces;

public interface IReportRepository
{
    Task<InventoryReportDto> GetInventoryReportAsync(
        DateTimeOffset? from,
        DateTimeOffset? to,
        Guid? categoryId,
        string? department,
        DeviceStatus? status,
        CancellationToken cancellationToken);
}
