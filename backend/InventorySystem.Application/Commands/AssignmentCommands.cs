using MediatR;

namespace InventorySystem.Application.Commands;

public record AssignDeviceCommand(
    Guid DeviceId,
    Guid PersonnelId,
    string? Note = null) : IRequest;

public record UnassignDeviceCommand(
    Guid DeviceId,
    string? Note = null) : IRequest;
