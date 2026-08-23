using InventorySystem.Application.Common.Exceptions;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Queries;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class GetPersonnelListQueryHandler(IPersonnelRepository repository)
    : IRequestHandler<GetPersonnelListQuery, IReadOnlyList<PersonnelListItemDto>>
{
    public async Task<IReadOnlyList<PersonnelListItemDto>> Handle(
        GetPersonnelListQuery request,
        CancellationToken cancellationToken)
    {
        var personnel = await repository.ListAsync(cancellationToken);
        return personnel.Select(item => item.ToListItemDto()).ToArray();
    }
}

public sealed class GetPersonnelByIdQueryHandler(IPersonnelRepository repository)
    : IRequestHandler<GetPersonnelByIdQuery, PersonnelDetailDto>
{
    public async Task<PersonnelDetailDto> Handle(
        GetPersonnelByIdQuery request,
        CancellationToken cancellationToken)
    {
        var personnel = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Personel kaydı bulunamadı.");

        return personnel.ToDetailDto();
    }
}
