using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;

namespace InventorySystem.Application.Interfaces;

public interface IWarrantyStatusService
{
    int ExpiringSoonDays { get; }
    WarrantyStatusResult Calculate(Device device);
}

public sealed record WarrantyStatusResult(
    WarrantyStatus Status,
    string StatusName,
    int? RemainingDays,
    string Message);
