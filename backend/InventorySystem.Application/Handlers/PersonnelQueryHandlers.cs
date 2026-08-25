using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Queries;
using MediatR;
namespace InventorySystem.Application.Handlers;
public sealed class GetPersonnelListQueryHandler(IPersonnelRepository repository) : IRequestHandler<GetPersonnelListQuery, PersonnelListResponse> { public async Task<PersonnelListResponse> Handle(GetPersonnelListQuery r, CancellationToken ct) { var x=await repository.ListAsync(r.Search,r.Page,r.PageSize,ct); return new(x.Items.Select(p=>p.ToListItemDto()).ToArray(),x.Page,x.PageSize,x.TotalCount,x.TotalPages,x.ActiveCount,x.AssignedDeviceCount); } }
public sealed class GetPersonnelByIdQueryHandler(IPersonnelRepository repository) : IRequestHandler<GetPersonnelByIdQuery, PersonnelDetailDto> { public async Task<PersonnelDetailDto> Handle(GetPersonnelByIdQuery r,CancellationToken ct) => (await repository.GetByIdAsync(r.Id,ct) ?? throw new NotFoundException("Personel kaydı bulunamadı.")).ToDetailDto(); }
