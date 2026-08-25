using InventorySystem.Application.DTOs;
using InventorySystem.Domain.Enums;
using MediatR;

namespace InventorySystem.Application.Queries;

public sealed record GetInventoryReportQuery(
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    Guid? CategoryId = null,
    string? Department = null,
    DeviceStatus? Status = null) : IRequest<InventoryReportDto>;

public sealed record ExportInventoryReportQuery(
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    Guid? CategoryId = null,
    string? Department = null,
    DeviceStatus? Status = null) : IRequest<ExcelFileDto>;
