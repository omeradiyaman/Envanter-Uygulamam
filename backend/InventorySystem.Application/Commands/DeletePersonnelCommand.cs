using MediatR;

namespace InventorySystem.Application.Commands;

public sealed record DeletePersonnelCommand(Guid Id) : IRequest;
