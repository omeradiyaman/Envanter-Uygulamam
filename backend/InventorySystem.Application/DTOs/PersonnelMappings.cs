using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.DTOs;

internal static class PersonnelMappings
{
    public static PersonnelListItemDto ToListItemDto(this Personnel personnel)
    {
        return new PersonnelListItemDto(
            personnel.Id,
            personnel.SicilNo,
            personnel.Ad,
            personnel.Soyad,
            personnel.Departman,
            personnel.Pozisyon,
            personnel.ZimmetNo,
            personnel.AktifMi);
    }

    public static PersonnelDetailDto ToDetailDto(this Personnel personnel)
    {
        return new PersonnelDetailDto(
            personnel.Id,
            personnel.SicilNo,
            personnel.Ad,
            personnel.Soyad,
            personnel.Departman,
            personnel.Pozisyon,
            personnel.ZimmetNo,
            personnel.AktifMi,
            personnel.CreatedAt,
            personnel.UpdatedAt);
    }
}
