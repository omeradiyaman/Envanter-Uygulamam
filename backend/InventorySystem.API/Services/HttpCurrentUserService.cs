using System.Security.Claims;
using InventorySystem.Application.Interfaces;

namespace InventorySystem.API.Services;

public sealed class HttpCurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;
    public Guid? UserId => Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public string? UserName => User?.Identity?.Name;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
}
