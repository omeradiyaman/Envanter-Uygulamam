namespace InventorySystem.Application.DTOs;

public sealed record ReportCountDto(string Key, string Label, int Count);
public sealed record ReportDeviceDto(
    Guid Id, string DeviceName, string InventoryNumber, string SerialNumber,
    string Category, string Status, string? Department, string WarrantyStatus);
public sealed record ReportAssignmentDto(
    Guid Id, string DeviceName, string InventoryNumber, string PersonnelName,
    string Department, DateTimeOffset AssignedAt, DateTimeOffset? ReturnedAt);
public sealed record ReportFilterOptionsDto(
    IReadOnlyList<ReportCountDto> Categories,
    IReadOnlyList<string> Departments);
public sealed record InventoryReportDto(
    IReadOnlyList<ReportCountDto> ByCategory,
    IReadOnlyList<ReportCountDto> ByStatus,
    IReadOnlyList<ReportCountDto> ByDepartment,
    IReadOnlyList<ReportDeviceDto> StockDevices,
    IReadOnlyList<ReportDeviceDto> ScrapDevices,
    IReadOnlyList<ReportDeviceDto> ExpiringWarranties,
    IReadOnlyList<ReportDeviceDto> ExpiredWarranties,
    IReadOnlyList<ReportAssignmentDto> AssignmentMovements,
    ReportFilterOptionsDto FilterOptions,
    DateTimeOffset GeneratedAt);
