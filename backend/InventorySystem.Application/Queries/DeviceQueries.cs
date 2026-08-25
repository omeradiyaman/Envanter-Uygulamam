using InventorySystem.Application.DTOs;
using MediatR;

namespace InventorySystem.Application.Queries;

public record GetDeviceListQuery : IRequest<IReadOnlyList<DeviceListDto>>;

public record GetDeviceByIdQuery(Guid Id) : IRequest<DeviceDto>;

public record GetDeviceQrCodeQuery(Guid Id) : IRequest<DeviceQrCodeDto>;

public record GetDeviceCategoriesQuery : IRequest<IReadOnlyList<DeviceCategoryDto>>;

public sealed record GetExpiringWarrantiesQuery : IRequest<IReadOnlyList<WarrantyAlertDeviceDto>>;
public sealed record GetExpiredWarrantiesQuery : IRequest<IReadOnlyList<WarrantyAlertDeviceDto>>;
public sealed record GetDevicesWithoutWarrantyQuery : IRequest<IReadOnlyList<WarrantyAlertDeviceDto>>;
public sealed record GetWarrantySummaryQuery : IRequest<WarrantySummaryDto>;
