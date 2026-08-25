namespace InventorySystem.Application.DTOs;
public sealed record PersonnelListItemDto(Guid Id, string SicilNo, string AdSoyad, string Departman, string Pozisyon, string? Eposta, int CihazSayisi, bool AktifMi);
public sealed record PersonnelListResponse(IReadOnlyList<PersonnelListItemDto> Items, int Page, int PageSize, int TotalCount, int TotalPages, int ActiveCount, int AssignedDeviceCount);
public sealed record PersonnelDetailDto(Guid Id, string SicilNo, string AdSoyad, string Departman, string Pozisyon, string? Eposta, IReadOnlyList<string> ZimmetliCihazlar, bool AktifMi, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public sealed record CreatePersonnelRequest(string SicilNo, string Ad, string Soyad, string Departman, string Pozisyon, string? Eposta, string? ZimmetNo, bool AktifMi);
public sealed record UpdatePersonnelRequest(string SicilNo, string Ad, string Soyad, string Departman, string Pozisyon, string? Eposta, string? ZimmetNo, bool AktifMi);
