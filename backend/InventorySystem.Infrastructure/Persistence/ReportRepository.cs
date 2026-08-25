using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Persistence;

public sealed class ReportRepository(
    ApplicationDbContext dbContext,
    IWarrantyStatusService warrantyStatusService,
    IDateTimeProvider dateTimeProvider) : IReportRepository
{
    public async Task<InventoryReportDto> GetInventoryReportAsync(
        DateTimeOffset? from, DateTimeOffset? to, Guid? categoryId, string? department,
        DeviceStatus? status, CancellationToken cancellationToken)
    {
        var allCategories = await dbContext.DeviceCategories.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var allDepartments = await dbContext.Personnel.AsNoTracking().Select(x => x.Departman).Distinct().OrderBy(x => x).ToListAsync(cancellationToken);
        var query = dbContext.Devices.AsNoTracking().Include(x => x.Category).Include(x => x.Personel).AsQueryable();
        if (from.HasValue) query = query.Where(x => x.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.CreatedAt <= to.Value);
        if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(department)) query = query.Where(x => x.Personel != null && x.Personel.Departman == department);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        var devices = await query.OrderBy(x => x.CihazAdi).ToListAsync(cancellationToken);

        var assignmentsQuery = dbContext.AssignmentHistories.AsNoTracking()
            .Include(x => x.Device).ThenInclude(x => x.Category)
            .Include(x => x.Personnel).AsQueryable();
        if (from.HasValue) assignmentsQuery = assignmentsQuery.Where(x => x.AssignedAt >= from.Value);
        if (to.HasValue) assignmentsQuery = assignmentsQuery.Where(x => x.AssignedAt <= to.Value);
        if (categoryId.HasValue) assignmentsQuery = assignmentsQuery.Where(x => x.Device.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(department)) assignmentsQuery = assignmentsQuery.Where(x => x.Personnel.Departman == department);
        if (status.HasValue) assignmentsQuery = assignmentsQuery.Where(x => x.Device.Status == status.Value);
        var assignments = await assignmentsQuery.OrderByDescending(x => x.AssignedAt).Take(500).ToListAsync(cancellationToken);

        var categoryCounts = devices.GroupBy(x => new { x.CategoryId, x.Category.Name })
            .Select(x => new ReportCountDto(x.Key.CategoryId.ToString(), x.Key.Name, x.Count())).OrderByDescending(x => x.Count).ToList();
        var statusCounts = devices.GroupBy(x => x.Status)
            .Select(x => new ReportCountDto(((int)x.Key).ToString(), StatusName(x.Key), x.Count())).OrderBy(x => x.Key).ToList();
        var departmentCounts = devices.Where(x => x.Status == DeviceStatus.Assigned && x.Personel != null)
            .GroupBy(x => x.Personel!.Departman)
            .Select(x => new ReportCountDto(x.Key, x.Key, x.Count())).OrderByDescending(x => x.Count).ToList();
        var rows = devices.Select(MapDevice).ToList();

        return new InventoryReportDto(
            categoryCounts, statusCounts, departmentCounts,
            rows.Where(x => x.Status == StatusName(DeviceStatus.InStock)).ToList(),
            rows.Where(x => x.Status == StatusName(DeviceStatus.Scrap)).ToList(),
            devices.Where(x => warrantyStatusService.Calculate(x).Status == WarrantyStatus.ExpiringSoon).Select(MapDevice).ToList(),
            devices.Where(x => warrantyStatusService.Calculate(x).Status == WarrantyStatus.Expired).Select(MapDevice).ToList(),
            assignments.Select(x => new ReportAssignmentDto(x.Id, x.Device.CihazAdi, x.Device.EnvanterNo,
                $"{x.Personnel.Ad} {x.Personnel.Soyad}", x.Personnel.Departman, x.AssignedAt, x.ReturnedAt)).ToList(),
            new ReportFilterOptionsDto(
                allCategories.Select(x => new ReportCountDto(x.Id.ToString(), x.Name, 0)).ToList(), allDepartments),
            dateTimeProvider.UtcNow);
    }

    private ReportDeviceDto MapDevice(Device device)
    {
        var warranty = warrantyStatusService.Calculate(device);
        return new ReportDeviceDto(device.Id, device.CihazAdi, device.EnvanterNo, device.SeriNo,
            device.Category.Name, StatusName(device.Status), device.Personel?.Departman, warranty.StatusName);
    }

    private static string StatusName(DeviceStatus status) => status switch
    {
        DeviceStatus.InStock => "Stokta", DeviceStatus.Assigned => "Zimmetli",
        DeviceStatus.InService => "Serviste", DeviceStatus.Scrap => "Hurda / İmha",
        DeviceStatus.Lost => "Kayıp", DeviceStatus.Passive => "Pasif", _ => status.ToString()
    };
}
