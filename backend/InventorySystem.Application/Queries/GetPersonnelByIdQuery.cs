using InventorySystem.Application.DTOs;
using MediatR;

namespace InventorySystem.Application.Queries;

public sealed record GetPersonnelByIdQuery(Guid Id) : IRequest<PersonnelDetailDto>;
