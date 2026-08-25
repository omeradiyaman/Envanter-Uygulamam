using InventorySystem.Application.DTOs;
using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.Common.Audit;

public static class AuditActionTypes
{
    public const string PersonnelCreated = "PersonnelCreated";
    public const string PersonnelUpdated = "PersonnelUpdated";
    public const string PersonnelDeleted = "PersonnelDeleted";
    public const string DeviceCreated = "DeviceCreated";
    public const string DeviceUpdated = "DeviceUpdated";
    public const string DeviceDeleted = "DeviceDeleted";
    public const string WarrantyUpdated = "WarrantyUpdated";
    public const string DeviceScrapped = "DeviceScrapped";
    public const string DeviceAssigned = "DeviceAssigned";
    public const string DeviceReturned = "DeviceReturned";
    public const string ExcelImported = "ExcelImported";
    public const string DocumentUploaded = "DocumentUploaded";
    public const string DocumentDeleted = "DocumentDeleted";
    public const string Undo = "Undo";
}

public static class AuditEntityTypes
{
    public const string Personnel = "Personnel";
    public const string Device = "Device";
    public const string Assignment = "Assignment";
    public const string AssignmentDocument = "AssignmentDocument";
    public const string ExcelBatch = "ExcelBatch";
}

public static class AuditSnapshots
{
    public static PersonnelAuditSnapshot Personnel(Personnel item) => new(
        item.SicilNo, item.Ad, item.Soyad, item.Departman, item.Pozisyon,
        item.ZimmetNo, item.AktifMi, item.IsDeleted);

    public static DeviceAuditSnapshot Device(Device item) => new(
        item.CihazAdi, item.SeriNo, item.EnvanterNo, item.BarkodNo,
        item.Marka, item.Model, item.CategoryId, (int)item.Status, item.PersonelId,
        item.WarrantyStartDate, item.WarrantyEndDate, item.WarrantyProvider,
        item.WarrantyNote, item.IsDeleted);

    public static AssignmentAuditSnapshot Assignment(AssignmentHistory item) => new(
        item.Id, item.DeviceId, item.PersonnelId, item.AssignedAt, item.ReturnedAt);
}
