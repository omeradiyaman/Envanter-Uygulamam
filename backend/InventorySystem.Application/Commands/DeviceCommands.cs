using InventorySystem.Domain.Enums;
using MediatR;

namespace InventorySystem.Application.Commands;

public record CreateDeviceCommand(
    string CihazAdi,
    string SeriNo,
    string EnvanterNo,
    string? BarkodNo,
    string Marka,
    string Model,
    Guid CategoryId,
    DeviceStatus Status,
    Guid? PersonelId,
    DateOnly? WarrantyStartDate,
    DateOnly? WarrantyEndDate,
    string? WarrantyProvider,
    string? WarrantyNote) : IRequest<Guid>;

public record UpdateDeviceCommand(
    Guid Id,
    string CihazAdi,
    string SeriNo,
    string EnvanterNo,
    string? BarkodNo,
    string Marka,
    string Model,
    Guid CategoryId,
    DeviceStatus Status,
    Guid? PersonelId,
    DateOnly? WarrantyStartDate,
    DateOnly? WarrantyEndDate,
    string? WarrantyProvider,
    string? WarrantyNote) : IRequest;

public record DeleteDeviceCommand(Guid Id) : IRequest;

public sealed record CreateDeviceCategoryCommand(string Name, string? Description) : IRequest<Guid>;
public sealed record UpdateDeviceCategoryCommand(Guid Id, string Name, string? Description) : IRequest;
public sealed record DeleteDeviceCategoryCommand(Guid Id) : IRequest;
