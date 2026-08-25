using InventorySystem.Domain.Common;
using InventorySystem.Domain.Enums;

namespace InventorySystem.Domain.Entities;

public sealed class Device : BaseEntity
{
    private Device()
    {
    }

    public Device(
        string cihazAdi,
        string seriNo,
        string envanterNo,
        string? barkodNo,
        string marka,
        string model,
        Guid categoryId,
        DeviceStatus status,
        Guid? personelId,
        DateOnly? warrantyStartDate = null,
        DateOnly? warrantyEndDate = null,
        string? warrantyProvider = null,
        string? warrantyNote = null)
    {
        ApplyChanges(cihazAdi, seriNo, envanterNo, barkodNo, marka, model, categoryId, status, personelId);
        ApplyWarranty(warrantyStartDate, warrantyEndDate, warrantyProvider, warrantyNote);
    }

    public string CihazAdi { get; private set; } = string.Empty;
    public string SeriNo { get; private set; } = string.Empty;
    public string EnvanterNo { get; private set; } = string.Empty;
    public string? BarkodNo { get; private set; }
    public string Marka { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public DateOnly? WarrantyStartDate { get; private set; }
    public DateOnly? WarrantyEndDate { get; private set; }
    public string? WarrantyProvider { get; private set; }
    public string? WarrantyNote { get; private set; }
    
    public Guid CategoryId { get; private set; }
    public DeviceCategory Category { get; private set; } = null!;

    public DeviceStatus Status { get; private set; }

    public Guid? PersonelId { get; private set; }
    public Personnel? Personel { get; private set; }

    private readonly List<AssignmentHistory> _assignmentHistory = new();
    public IReadOnlyCollection<AssignmentHistory> AssignmentHistory => _assignmentHistory.AsReadOnly();

    public void Update(
        string cihazAdi,
        string seriNo,
        string envanterNo,
        string? barkodNo,
        string marka,
        string model,
        Guid categoryId,
        DeviceStatus status,
        Guid? personelId,
        DateTimeOffset updatedAt)
    {
        ApplyChanges(cihazAdi, seriNo, envanterNo, barkodNo, marka, model, categoryId, status, personelId);
        UpdatedAt = updatedAt;
    }

    public void Update(
        string cihazAdi,
        string seriNo,
        string envanterNo,
        string? barkodNo,
        string marka,
        string model,
        Guid categoryId,
        DeviceStatus status,
        Guid? personelId,
        DateTimeOffset updatedAt,
        DateOnly? warrantyStartDate,
        DateOnly? warrantyEndDate,
        string? warrantyProvider,
        string? warrantyNote)
    {
        Update(cihazAdi, seriNo, envanterNo, barkodNo, marka, model, categoryId, status, personelId, updatedAt);
        ApplyWarranty(warrantyStartDate, warrantyEndDate, warrantyProvider, warrantyNote);
    }

    public void Assign(Guid personnelId, DateTimeOffset updatedAt)
    {
        PersonelId = personnelId;
        Status = DeviceStatus.Assigned;
        UpdatedAt = updatedAt;
    }

    public void Unassign(DateTimeOffset updatedAt)
    {
        PersonelId = null;
        Status = DeviceStatus.InStock;
        UpdatedAt = updatedAt;
    }

    public void UpdateStatus(DeviceStatus newStatus, DateTimeOffset updatedAt)
    {
        Status = newStatus;
        UpdatedAt = updatedAt;
    }

    public void SoftDelete(DateTimeOffset deletedAt)
    {
        IsDeleted = true;
        DeletedAt = deletedAt;
        UpdatedAt = deletedAt;
    }

    public void RestoreFromDelete(DateTimeOffset restoredAt) => Restore(restoredAt);

    private void ApplyChanges(
        string cihazAdi,
        string seriNo,
        string envanterNo,
        string? barkodNo,
        string marka,
        string model,
        Guid categoryId,
        DeviceStatus status,
        Guid? personelId)
    {
        CihazAdi = cihazAdi.Trim();
        SeriNo = seriNo.Trim().ToUpperInvariant();
        EnvanterNo = envanterNo.Trim().ToUpperInvariant();
        BarkodNo = string.IsNullOrWhiteSpace(barkodNo) ? null : barkodNo.Trim().ToUpperInvariant();
        Marka = marka.Trim();
        Model = model.Trim();
        CategoryId = categoryId;
        Status = status;
        PersonelId = personelId;
    }

    private void ApplyWarranty(
        DateOnly? warrantyStartDate,
        DateOnly? warrantyEndDate,
        string? warrantyProvider,
        string? warrantyNote)
    {
        WarrantyStartDate = warrantyStartDate;
        WarrantyEndDate = warrantyEndDate;
        WarrantyProvider = string.IsNullOrWhiteSpace(warrantyProvider) ? null : warrantyProvider.Trim();
        WarrantyNote = string.IsNullOrWhiteSpace(warrantyNote) ? null : warrantyNote.Trim();
    }
}
