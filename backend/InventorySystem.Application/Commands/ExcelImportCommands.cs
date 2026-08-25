using InventorySystem.Application.DTOs;
using MediatR;

namespace InventorySystem.Application.Commands;

public sealed record PreviewDeviceImportCommand(
    string FileName,
    byte[] Content) : IRequest<ImportPreviewSummaryDto>;

public sealed record ImportDevicesCommand(
    string FileName,
    byte[] Content) : IRequest<ImportExecutionResultDto>;

public sealed record PreviewPersonnelImportCommand(
    string FileName,
    byte[] Content) : IRequest<ImportPreviewSummaryDto>;

public sealed record ImportPersonnelCommand(
    string FileName,
    byte[] Content) : IRequest<ImportExecutionResultDto>;
