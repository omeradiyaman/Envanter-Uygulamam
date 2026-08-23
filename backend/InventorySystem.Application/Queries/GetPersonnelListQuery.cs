using InventorySystem.Application.DTOs;
using MediatR;

namespace InventorySystem.Application.Queries;

public sealed record GetPersonnelListQuery : IRequest<IReadOnlyList<PersonnelListItemDto>>;
