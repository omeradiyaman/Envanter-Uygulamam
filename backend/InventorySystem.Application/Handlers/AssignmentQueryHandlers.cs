using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Queries;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class AssignmentQueryHandlers(IAssignmentRepository assignmentRepository)
    : IRequestHandler<GetDeviceAssignmentHistoryQuery, IReadOnlyList<AssignmentHistoryDto>>,
      IRequestHandler<GetPersonnelAssignmentHistoryQuery, IReadOnlyList<AssignmentHistoryDto>>
{
    public async Task<IReadOnlyList<AssignmentHistoryDto>> Handle(
        GetDeviceAssignmentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var records = await assignmentRepository.GetDeviceHistoryAsync(
            request.DeviceId, cancellationToken);

        return records.Select(a => new AssignmentHistoryDto(
            a.Id,
            a.DeviceId,
            a.Device.CihazAdi,
            a.Device.SeriNo,
            a.Device.EnvanterNo,
            a.Device.Category?.Name ?? "Bilinmiyor",
            a.PersonnelId,
            $"{a.Personnel.Ad} {a.Personnel.Soyad}",
            a.Personnel.SicilNo,
            a.AssignedAt,
            a.ReturnedAt,
            a.Note,
            a.IsActive
        )).ToList();
    }

    public async Task<IReadOnlyList<AssignmentHistoryDto>> Handle(
        GetPersonnelAssignmentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var records = await assignmentRepository.GetPersonnelHistoryAsync(
            request.PersonnelId, cancellationToken);

        return records.Select(a => new AssignmentHistoryDto(
            a.Id,
            a.DeviceId,
            a.Device.CihazAdi,
            a.Device.SeriNo,
            a.Device.EnvanterNo,
            a.Device.Category?.Name ?? "Bilinmiyor",
            a.PersonnelId,
            $"{a.Personnel.Ad} {a.Personnel.Soyad}",
            a.Personnel.SicilNo,
            a.AssignedAt,
            a.ReturnedAt,
            a.Note,
            a.IsActive
        )).ToList();
    }
}
