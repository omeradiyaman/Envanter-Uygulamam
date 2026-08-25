using InventorySystem.Domain.Common;

namespace InventorySystem.Domain.Entities;

public sealed class Personnel : BaseEntity
{
    private Personnel()
    {
    }

    public Personnel(
        string sicilNo,
        string ad,
        string soyad,
        string departman,
        string pozisyon,
        string? zimmetNo,
        bool aktifMi)
    {
        ApplyChanges(sicilNo, ad, soyad, departman, pozisyon, zimmetNo, aktifMi);
    }

    public string SicilNo { get; private set; } = string.Empty;

    public string Ad { get; private set; } = string.Empty;

    public string Soyad { get; private set; } = string.Empty;

    public string Departman { get; private set; } = string.Empty;

    public string Pozisyon { get; private set; } = string.Empty;

    public string? ZimmetNo { get; private set; }

    public bool AktifMi { get; private set; }

    private readonly List<Device> _devices = new();
    public IReadOnlyCollection<Device> Devices => _devices.AsReadOnly();

    public void Update(
        string sicilNo,
        string ad,
        string soyad,
        string departman,
        string pozisyon,
        string? zimmetNo,
        bool aktifMi,
        DateTimeOffset updatedAt)
    {
        ApplyChanges(sicilNo, ad, soyad, departman, pozisyon, zimmetNo, aktifMi);
        UpdatedAt = updatedAt;
    }

    public void SoftDelete(DateTimeOffset deletedAt)
    {
        IsDeleted = true;
        DeletedAt = deletedAt;
        UpdatedAt = deletedAt;
        AktifMi = false;
    }

    public void RestoreFromDelete(bool aktifMi, DateTimeOffset restoredAt)
    {
        Restore(restoredAt);
        AktifMi = aktifMi;
    }

    private void ApplyChanges(
        string sicilNo,
        string ad,
        string soyad,
        string departman,
        string pozisyon,
        string? zimmetNo,
        bool aktifMi)
    {
        SicilNo = sicilNo.Trim().ToUpperInvariant();
        Ad = ad.Trim();
        Soyad = soyad.Trim();
        Departman = departman.Trim();
        Pozisyon = pozisyon.Trim();
        ZimmetNo = string.IsNullOrWhiteSpace(zimmetNo) ? null : zimmetNo.Trim();
        AktifMi = aktifMi;
    }
}
