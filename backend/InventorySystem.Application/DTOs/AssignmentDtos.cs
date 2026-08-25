namespace InventorySystem.Application.DTOs;

public record AssignmentHistoryDto(
    Guid Id,
    Guid DeviceId,
    string DeviceName,
    string DeviceSeriNo,
    string DeviceEnvanterNo,
    string DeviceCategoryName,
    Guid PersonnelId,
    string PersonnelName,
    string PersonnelSicilNo,
    DateTimeOffset AssignedAt,
    DateTimeOffset? ReturnedAt,
    string? Note,
    bool IsActive);
