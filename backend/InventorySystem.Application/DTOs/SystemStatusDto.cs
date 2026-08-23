namespace InventorySystem.Application.DTOs;

public sealed record SystemStatusDto(
    string Status,
    string Service,
    string Client,
    DateTimeOffset Timestamp,
    string ApiVersion);
