using ClosedXML.Excel;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Queries;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class GetInventoryReportQueryHandler(IReportRepository repository)
    : IRequestHandler<GetInventoryReportQuery, InventoryReportDto>
{
    public Task<InventoryReportDto> Handle(GetInventoryReportQuery request, CancellationToken cancellationToken) =>
        repository.GetInventoryReportAsync(request.From, request.To, request.CategoryId, request.Department, request.Status, cancellationToken);
}

public sealed class ExportInventoryReportQueryHandler(IReportRepository repository)
    : IRequestHandler<ExportInventoryReportQuery, ExcelFileDto>
{
    public async Task<ExcelFileDto> Handle(ExportInventoryReportQuery request, CancellationToken cancellationToken)
    {
        var report = await repository.GetInventoryReportAsync(request.From, request.To, request.CategoryId, request.Department, request.Status, cancellationToken);
        using var workbook = new XLWorkbook();
        AddCounts(workbook, "Kategori", report.ByCategory);
        AddCounts(workbook, "Durum", report.ByStatus);
        AddCounts(workbook, "Departman Zimmet", report.ByDepartment);
        AddDevices(workbook, "Stok", report.StockDevices);
        AddDevices(workbook, "Hurda İmha", report.ScrapDevices);
        AddDevices(workbook, "Yaklaşan Garanti", report.ExpiringWarranties);
        AddDevices(workbook, "Biten Garanti", report.ExpiredWarranties);
        var assignmentSheet = workbook.Worksheets.Add("Zimmet Hareketleri");
        assignmentSheet.Cell(1, 1).Value = "Cihaz"; assignmentSheet.Cell(1, 2).Value = "Envanter No";
        assignmentSheet.Cell(1, 3).Value = "Personel"; assignmentSheet.Cell(1, 4).Value = "Departman";
        assignmentSheet.Cell(1, 5).Value = "Zimmet Tarihi"; assignmentSheet.Cell(1, 6).Value = "İade Tarihi";
        for (var i = 0; i < report.AssignmentMovements.Count; i++)
        {
            var row = report.AssignmentMovements[i]; var n = i + 2;
            assignmentSheet.Cell(n, 1).Value = row.DeviceName; assignmentSheet.Cell(n, 2).Value = row.InventoryNumber;
            assignmentSheet.Cell(n, 3).Value = row.PersonnelName; assignmentSheet.Cell(n, 4).Value = row.Department;
            assignmentSheet.Cell(n, 5).Value = row.AssignedAt.UtcDateTime;
            if (row.ReturnedAt.HasValue) assignmentSheet.Cell(n, 6).Value = row.ReturnedAt.Value.UtcDateTime;
        }
        Format(workbook);
        using var stream = new MemoryStream(); workbook.SaveAs(stream);
        return new ExcelFileDto($"envanter-raporu-{DateTime.UtcNow:yyyyMMdd-HHmm}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", stream.ToArray());
    }

    private static void AddCounts(XLWorkbook workbook, string name, IReadOnlyList<ReportCountDto> rows)
    {
        var sheet = workbook.Worksheets.Add(name); sheet.Cell(1, 1).Value = "Grup"; sheet.Cell(1, 2).Value = "Adet";
        for (var i = 0; i < rows.Count; i++) { sheet.Cell(i + 2, 1).Value = rows[i].Label; sheet.Cell(i + 2, 2).Value = rows[i].Count; }
    }

    private static void AddDevices(XLWorkbook workbook, string name, IReadOnlyList<ReportDeviceDto> rows)
    {
        var sheet = workbook.Worksheets.Add(name);
        string[] headers = ["Cihaz", "Envanter No", "Seri No", "Kategori", "Durum", "Departman", "Garanti"];
        for (var j = 0; j < headers.Length; j++) sheet.Cell(1, j + 1).Value = headers[j];
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i]; var n = i + 2;
            sheet.Cell(n, 1).Value = row.DeviceName; sheet.Cell(n, 2).Value = row.InventoryNumber;
            sheet.Cell(n, 3).Value = row.SerialNumber; sheet.Cell(n, 4).Value = row.Category;
            sheet.Cell(n, 5).Value = row.Status; sheet.Cell(n, 6).Value = row.Department ?? "";
            sheet.Cell(n, 7).Value = row.WarrantyStatus;
        }
    }

    private static void Format(XLWorkbook workbook)
    {
        foreach (var sheet in workbook.Worksheets)
        {
            var used = sheet.RangeUsed(); if (used is null) continue;
            sheet.Row(1).Style.Font.Bold = true; sheet.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E9F0FC");
            used.SetAutoFilter(); sheet.SheetView.FreezeRows(1); sheet.Columns().AdjustToContents();
        }
    }
}
