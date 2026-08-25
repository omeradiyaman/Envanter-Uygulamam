using InventorySystem.Application.DTOs;
using InventorySystem.Domain.Enums;
using MediatR;

namespace InventorySystem.Application.Queries;

public sealed record ExportDevicesQuery(
    string? SearchTerm,
    Guid? CategoryId,
    DeviceStatus? Status,
    IReadOnlyList<Guid>? Ids = null) : IRequest<ExcelFileDto>;

public sealed record ExportPersonnelQuery(
    string? SearchTerm,
    bool? AktifMi,
    IReadOnlyList<Guid>? Ids = null) : IRequest<ExcelFileDto>;

public sealed record ExportAllInventoryQuery : IRequest<ExcelFileDto>;
