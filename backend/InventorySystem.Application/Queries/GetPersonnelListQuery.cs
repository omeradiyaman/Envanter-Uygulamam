using InventorySystem.Application.DTOs;
using MediatR;
namespace InventorySystem.Application.Queries;
public sealed record GetPersonnelListQuery(string? Search, int Page = 1, int PageSize = 20) : IRequest<PersonnelListResponse>;
