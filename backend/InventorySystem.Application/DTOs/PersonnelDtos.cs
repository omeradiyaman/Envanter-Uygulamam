namespace InventorySystem.Application.DTOs;

public sealed record PersonnelListItemDto(
    Guid Id,
    string SicilNo,
    string Ad,
    string Soyad,
    string Departman,
    string Pozisyon,
    string? ZimmetNo,
    bool AktifMi);

public sealed record PersonnelDetailDto(
    Guid Id,
    string SicilNo,
    string Ad,
    string Soyad,
    string Departman,
    string Pozisyon,
    string? ZimmetNo,
    bool AktifMi,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<DeviceListDto> Devices);

public sealed record CreatePersonnelRequest(
    string SicilNo,
    string Ad,
    string Soyad,
    string Departman,
    string Pozisyon,
    string? ZimmetNo,
    bool AktifMi);

public sealed record UpdatePersonnelRequest(
    string SicilNo,
    string Ad,
    string Soyad,
    string Departman,
    string Pozisyon,
    string? ZimmetNo,
    bool AktifMi);
