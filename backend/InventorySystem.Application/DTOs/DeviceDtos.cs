using InventorySystem.Domain.Enums;

namespace InventorySystem.Application.DTOs;

public record DeviceDto(
    Guid Id,
    string CihazAdi,
    string SeriNo,
    string EnvanterNo,
    string? BarkodNo,
    string Marka,
    string Model,
    Guid CategoryId,
    string CategoryName,
    DeviceStatus Status,
    string StatusName,
    Guid? PersonelId,
    string? PersonelName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateOnly? WarrantyStartDate = null,
    DateOnly? WarrantyEndDate = null,
    string? WarrantyProvider = null,
    string? WarrantyNote = null,
    WarrantyStatus WarrantyStatus = WarrantyStatus.NoWarranty,
    string WarrantyStatusName = "Garanti bilgisi yok",
    int? WarrantyRemainingDays = null,
    string WarrantyMessage = "Garanti bitiş tarihi girilmemiş.");

public record DeviceListDto(
    Guid Id,
    string CihazAdi,
    string SeriNo,
    string EnvanterNo,
    string Marka,
    string Model,
    string CategoryName,
    DeviceStatus Status,
    string StatusName,
    string? PersonelName,
    DateTimeOffset CreatedAt,
    DateOnly? WarrantyStartDate = null,
    DateOnly? WarrantyEndDate = null,
    string? WarrantyProvider = null,
    string? WarrantyNote = null,
    WarrantyStatus WarrantyStatus = WarrantyStatus.NoWarranty,
    string WarrantyStatusName = "Garanti bilgisi yok",
    int? WarrantyRemainingDays = null,
    string WarrantyMessage = "Garanti bitiş tarihi girilmemiş.");

public record DeviceCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    int DeviceCount);

public record DeviceQrCodeDto(
    Guid DeviceId,
    string DeviceUrl,
    string PngBase64);

public sealed record WarrantyAlertDeviceDto(
    Guid Id,
    string CihazAdi,
    string SeriNo,
    string EnvanterNo,
    DateOnly? WarrantyEndDate,
    WarrantyStatus WarrantyStatus,
    string WarrantyStatusName,
    int? WarrantyRemainingDays,
    string WarrantyMessage);

public sealed record WarrantySummaryDto(
    int ExpiringSoonDays,
    int ExpiringCount,
    int ExpiredCount,
    int MissingCount);
