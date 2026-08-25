using InventorySystem.Application.DTOs;
using MediatR;

namespace InventorySystem.Application.Queries;

public record GetDeviceAssignmentHistoryQuery(Guid DeviceId)
    : IRequest<IReadOnlyList<AssignmentHistoryDto>>;

public record GetPersonnelAssignmentHistoryQuery(Guid PersonnelId)
    : IRequest<IReadOnlyList<AssignmentHistoryDto>>;
