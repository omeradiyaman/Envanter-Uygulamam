using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Queries;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class GetSystemStatusQueryHandler(IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetSystemStatusQuery, SystemStatusDto>
{
    public Task<SystemStatusDto> Handle(
        GetSystemStatusQuery request,
        CancellationToken cancellationToken)
    {
        var status = new SystemStatusDto(
            "healthy",
            "InventorySystem.API",
            request.Client,
            dateTimeProvider.UtcNow,
            "v1");

        return Task.FromResult(status);
    }
}
