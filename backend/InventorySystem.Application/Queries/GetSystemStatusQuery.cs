using InventorySystem.Application.DTOs;
using MediatR;

namespace InventorySystem.Application.Queries;

public sealed record GetSystemStatusQuery(string Client) : IRequest<SystemStatusDto>;
