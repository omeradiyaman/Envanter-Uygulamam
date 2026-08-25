using System.Globalization;
using ClosedXML.Excel;
using FluentValidation;
using FluentValidation.Results;
using InventorySystem.Application.Commands;
using InventorySystem.Application.Common.Audit;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.Queries;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using MediatR;

namespace InventorySystem.Application.Handlers;

public sealed class ExportDevicesQueryHandler(IDeviceRepository deviceRepository)
    : IRequestHandler<ExportDevicesQuery, ExcelFileDto>
{
    public async Task<ExcelFileDto> Handle(
        ExportDevicesQuery request,
        CancellationToken cancellationToken)
    {
        var devices = await deviceRepository.ListFilteredAsync(
            request.SearchTerm,
            request.CategoryId,
            request.Status,
            cancellationToken);
        if (request.Ids is { Count: > 0 })
        {
            var selectedIds = request.Ids.ToHashSet();
            devices = devices.Where(device => selectedIds.Contains(device.Id)).ToList();
        }

        using var workbook = new XLWorkbook();
        ExcelWorkbookFactory.AddDeviceWorksheet(workbook, "Cihazlar", devices);

        return ExcelWorkbookFactory.BuildFile(
            workbook,
            $"cihazlar_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }
}

public sealed class ExportPersonnelQueryHandler(IPersonnelRepository personnelRepository)
    : IRequestHandler<ExportPersonnelQuery, ExcelFileDto>
{
    public async Task<ExcelFileDto> Handle(
        ExportPersonnelQuery request,
        CancellationToken cancellationToken)
    {
        var personnel = await personnelRepository.ListFilteredAsync(
            request.SearchTerm,
            request.AktifMi,
            cancellationToken);
        if (request.Ids is { Count: > 0 })
        {
            var selectedIds = request.Ids.ToHashSet();
            personnel = personnel.Where(item => selectedIds.Contains(item.Id)).ToList();
        }

        using var workbook = new XLWorkbook();
        ExcelWorkbookFactory.AddPersonnelWorksheet(workbook, "Personeller", personnel);

        return ExcelWorkbookFactory.BuildFile(
            workbook,
            $"personeller_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }
}

public sealed class ExportAllInventoryQueryHandler(
    IDeviceRepository deviceRepository,
    IPersonnelRepository personnelRepository)
    : IRequestHandler<ExportAllInventoryQuery, ExcelFileDto>
{
    public async Task<ExcelFileDto> Handle(
        ExportAllInventoryQuery request,
        CancellationToken cancellationToken)
    {
        var devices = await deviceRepository.ListAsync(cancellationToken);
        var personnel = await personnelRepository.ListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        ExcelWorkbookFactory.AddPersonnelWorksheet(workbook, "Personeller", personnel);
        ExcelWorkbookFactory.AddDeviceWorksheet(workbook, "Tüm Cihazlar", devices);

        foreach (var categoryGroup in devices
                     .GroupBy(device => device.Category.Name)
                     .OrderBy(group => group.Key, StringComparer.CurrentCultureIgnoreCase))
        {
            ExcelWorkbookFactory.AddDeviceWorksheet(
                workbook,
                categoryGroup.Key,
                categoryGroup.ToList());
        }

        ExcelWorkbookFactory.AddDeviceWorksheet(
            workbook,
            "Stok",
            devices.Where(device => device.Status == DeviceStatus.InStock).ToList());

        ExcelWorkbookFactory.AddDeviceWorksheet(
            workbook,
            "Hurda - İmha",
            devices.Where(device => device.Status == DeviceStatus.Scrap).ToList());

        return ExcelWorkbookFactory.BuildFile(
            workbook,
            $"envanter_tam_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }
}

public sealed class PreviewDeviceImportCommandHandler(
    IDeviceRepository deviceRepository)
    : IRequestHandler<PreviewDeviceImportCommand, ImportPreviewSummaryDto>
{
    public async Task<ImportPreviewSummaryDto> Handle(
        PreviewDeviceImportCommand request,
        CancellationToken cancellationToken)
    {
        var categories = await deviceRepository.ListCategoriesAsync(cancellationToken);
        var devices = await deviceRepository.ListAsync(cancellationToken);
        var rows = ExcelImportParser.ParseDeviceRows(request.Content);
        var plans = ExcelImportAnalyzer.AnalyzeDevices(rows, devices, categories);

        return ExcelImportAnalyzer.BuildPreviewSummary(plans);
    }
}

public sealed class ImportDevicesCommandHandler(
    IDeviceRepository deviceRepository,
    IDateTimeProvider dateTimeProvider,
    IAuditLogService auditLogService)
    : IRequestHandler<ImportDevicesCommand, ImportExecutionResultDto>
{
    public async Task<ImportExecutionResultDto> Handle(
        ImportDevicesCommand request,
        CancellationToken cancellationToken)
    {
        var trackedDevices = await deviceRepository.ListForImportAsync(cancellationToken);
        var categories = await deviceRepository.ListCategoriesAsync(cancellationToken);
        var rows = ExcelImportParser.ParseDeviceRows(request.Content);
        var plans = ExcelImportAnalyzer.AnalyzeDevices(rows, trackedDevices, categories);

        var deviceLookup = trackedDevices.ToDictionary(device => device.Id);

        foreach (var plan in plans)
        {
            switch (plan.Action)
            {
                case ImportPlanAction.New:
                    var device = new Device(
                        plan.CihazAdi!,
                        plan.SeriNo!,
                        plan.EnvanterNo!,
                        plan.BarkodNo,
                        plan.Marka!,
                        plan.Model!,
                        plan.CategoryId!.Value,
                        plan.DeviceStatus!.Value,
                        plan.PersonelId);

                    await deviceRepository.AddAsync(device, cancellationToken);
                    break;

                case ImportPlanAction.Update:
                    var existingDevice = deviceLookup[plan.ExistingEntityId!.Value];
                    existingDevice.Update(
                        plan.CihazAdi!,
                        plan.SeriNo!,
                        plan.EnvanterNo!,
                        plan.BarkodNo,
                        plan.Marka!,
                        plan.Model!,
                        plan.CategoryId!.Value,
                        plan.DeviceStatus!.Value,
                        plan.PersonelId,
                        dateTimeProvider.UtcNow);
                    break;
            }
        }

        if (plans.Any(plan => plan.Action is ImportPlanAction.New or ImportPlanAction.Update))
        {
            var operationId = Guid.NewGuid();
            var created = plans.Count(plan => plan.Action == ImportPlanAction.New);
            var updated = plans.Count(plan => plan.Action == ImportPlanAction.Update);
            await auditLogService.RecordAsync(
                AuditActionTypes.ExcelImported, AuditEntityTypes.ExcelBatch, operationId,
                $"{request.FileName} dosyasından cihaz importu: {created} yeni, {updated} güncelleme.",
                null, new { request.FileName, CreatedCount = created, UpdatedCount = updated },
                false, cancellationToken, operationId: operationId);
            await deviceRepository.SaveChangesAsync(cancellationToken);
        }

        return ExcelImportAnalyzer.BuildExecutionResult(plans);
    }
}

public sealed class PreviewPersonnelImportCommandHandler(
    IPersonnelRepository personnelRepository)
    : IRequestHandler<PreviewPersonnelImportCommand, ImportPreviewSummaryDto>
{
    public async Task<ImportPreviewSummaryDto> Handle(
        PreviewPersonnelImportCommand request,
        CancellationToken cancellationToken)
    {
        var personnel = await personnelRepository.ListAsync(cancellationToken);
        var rows = ExcelImportParser.ParsePersonnelRows(request.Content);
        var plans = ExcelImportAnalyzer.AnalyzePersonnel(rows, personnel);

        return ExcelImportAnalyzer.BuildPreviewSummary(plans);
    }
}

public sealed class ImportPersonnelCommandHandler(
    IPersonnelRepository personnelRepository,
    IDateTimeProvider dateTimeProvider,
    IAuditLogService auditLogService)
    : IRequestHandler<ImportPersonnelCommand, ImportExecutionResultDto>
{
    public async Task<ImportExecutionResultDto> Handle(
        ImportPersonnelCommand request,
        CancellationToken cancellationToken)
    {
        var trackedPersonnel = await personnelRepository.ListForImportAsync(cancellationToken);
        var rows = ExcelImportParser.ParsePersonnelRows(request.Content);
        var plans = ExcelImportAnalyzer.AnalyzePersonnel(rows, trackedPersonnel);
        var personnelLookup = trackedPersonnel.ToDictionary(item => item.Id);

        foreach (var plan in plans)
        {
            switch (plan.Action)
            {
                case ImportPlanAction.New:
                    var personnel = new Personnel(
                        plan.SicilNo!,
                        plan.Ad!,
                        plan.Soyad!,
                        plan.Departman!,
                        plan.Pozisyon!,
                        plan.ZimmetNo,
                        plan.AktifMi!.Value);

                    await personnelRepository.AddAsync(personnel, cancellationToken);
                    break;

                case ImportPlanAction.Update:
                    var existingPersonnel = personnelLookup[plan.ExistingEntityId!.Value];
                    existingPersonnel.Update(
                        plan.SicilNo!,
                        plan.Ad!,
                        plan.Soyad!,
                        plan.Departman!,
                        plan.Pozisyon!,
                        plan.ZimmetNo,
                        plan.AktifMi!.Value,
                        dateTimeProvider.UtcNow);
                    break;
            }
        }

        if (plans.Any(plan => plan.Action is ImportPlanAction.New or ImportPlanAction.Update))
        {
            var operationId = Guid.NewGuid();
            var created = plans.Count(plan => plan.Action == ImportPlanAction.New);
            var updated = plans.Count(plan => plan.Action == ImportPlanAction.Update);
            await auditLogService.RecordAsync(
                AuditActionTypes.ExcelImported, AuditEntityTypes.ExcelBatch, operationId,
                $"{request.FileName} dosyasından personel importu: {created} yeni, {updated} güncelleme.",
                null, new { request.FileName, CreatedCount = created, UpdatedCount = updated },
                false, cancellationToken, operationId: operationId);
            await personnelRepository.SaveChangesAsync(cancellationToken);
        }

        return ExcelImportAnalyzer.BuildExecutionResult(plans);
    }
}

internal static class ExcelWorkbookFactory
{
    private const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static ExcelFileDto BuildFile(XLWorkbook workbook, string fileName)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new ExcelFileDto(fileName, ExcelContentType, stream.ToArray());
    }

    public static void AddDeviceWorksheet(
        XLWorkbook workbook,
        string name,
        IReadOnlyList<Device> devices)
    {
        var worksheet = workbook.Worksheets.Add(GetUniqueWorksheetName(workbook, name));

        worksheet.Cell(1, 1).Value = "Cihaz Adı";
        worksheet.Cell(1, 2).Value = "Kategori";
        worksheet.Cell(1, 3).Value = "Marka";
        worksheet.Cell(1, 4).Value = "Model";
        worksheet.Cell(1, 5).Value = "Seri No";
        worksheet.Cell(1, 6).Value = "Envanter No";
        worksheet.Cell(1, 7).Value = "Barkod No";
        worksheet.Cell(1, 8).Value = "Durum";
        worksheet.Cell(1, 9).Value = "Zimmetli Personel";
        worksheet.Cell(1, 10).Value = "Sicil No";
        worksheet.Cell(1, 11).Value = "Oluşturulma Tarihi";

        for (var index = 0; index < devices.Count; index++)
        {
            var row = index + 2;
            var device = devices[index];

            worksheet.Cell(row, 1).Value = device.CihazAdi;
            worksheet.Cell(row, 2).Value = device.Category.Name;
            worksheet.Cell(row, 3).Value = device.Marka;
            worksheet.Cell(row, 4).Value = device.Model;
            worksheet.Cell(row, 5).Value = device.SeriNo;
            worksheet.Cell(row, 6).Value = device.EnvanterNo;
            worksheet.Cell(row, 7).Value = device.BarkodNo ?? string.Empty;
            worksheet.Cell(row, 8).Value = ExcelImportAnalyzer.GetDeviceStatusDisplayName(device.Status);
            worksheet.Cell(row, 9).Value = device.Personel != null
                ? $"{device.Personel.Ad} {device.Personel.Soyad}"
                : string.Empty;
            worksheet.Cell(row, 10).Value = device.Personel?.SicilNo ?? string.Empty;
            worksheet.Cell(row, 11).Value = device.CreatedAt.LocalDateTime;
            worksheet.Cell(row, 11).Style.DateFormat.Format = "dd.MM.yyyy HH:mm";
        }

        FinalizeWorksheet(worksheet);
    }

    public static void AddPersonnelWorksheet(
        XLWorkbook workbook,
        string name,
        IReadOnlyList<Personnel> personnel)
    {
        var worksheet = workbook.Worksheets.Add(GetUniqueWorksheetName(workbook, name));

        worksheet.Cell(1, 1).Value = "Sicil No";
        worksheet.Cell(1, 2).Value = "Ad";
        worksheet.Cell(1, 3).Value = "Soyad";
        worksheet.Cell(1, 4).Value = "Departman";
        worksheet.Cell(1, 5).Value = "Pozisyon";
        worksheet.Cell(1, 6).Value = "Zimmet No";
        worksheet.Cell(1, 7).Value = "Aktif Mi";
        worksheet.Cell(1, 8).Value = "Zimmetli Cihaz Sayısı";

        for (var index = 0; index < personnel.Count; index++)
        {
            var row = index + 2;
            var item = personnel[index];

            worksheet.Cell(row, 1).Value = item.SicilNo;
            worksheet.Cell(row, 2).Value = item.Ad;
            worksheet.Cell(row, 3).Value = item.Soyad;
            worksheet.Cell(row, 4).Value = item.Departman;
            worksheet.Cell(row, 5).Value = item.Pozisyon;
            worksheet.Cell(row, 6).Value = item.ZimmetNo ?? string.Empty;
            worksheet.Cell(row, 7).Value = item.AktifMi ? "Evet" : "Hayır";
            worksheet.Cell(row, 8).Value = item.Devices.Count;
        }

        FinalizeWorksheet(worksheet);
    }

    private static void FinalizeWorksheet(IXLWorksheet worksheet)
    {
        var usedRange = worksheet.RangeUsed();
        if (usedRange == null)
        {
            return;
        }

        var header = worksheet.Range(1, 1, 1, usedRange.ColumnCount());
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#E9F0FC");
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        worksheet.SheetView.FreezeRows(1);
        usedRange.SetAutoFilter();
        worksheet.Columns().AdjustToContents();
    }

    private static string GetUniqueWorksheetName(XLWorkbook workbook, string name)
    {
        var sanitized = string.Concat(name.Where(character => !"[]:*?/\\'".Contains(character))).Trim();
        sanitized = string.IsNullOrWhiteSpace(sanitized) ? "Sayfa" : sanitized;
        sanitized = sanitized.Length > 31 ? sanitized[..31] : sanitized;

        var candidate = sanitized;
        var suffix = 1;

        while (workbook.Worksheets.Any(worksheet =>
                   worksheet.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase)))
        {
            var suffixText = $"_{suffix}";
            var maxBaseLength = Math.Max(1, 31 - suffixText.Length);
            candidate = $"{sanitized[..Math.Min(sanitized.Length, maxBaseLength)]}{suffixText}";
            suffix++;
        }

        return candidate;
    }
}

internal static class ExcelImportParser
{
    public static IReadOnlyList<DeviceImportRowInput> ParseDeviceRows(byte[] content)
    {
        var worksheet = GetWorksheet(content);
        var headers = BuildHeaderMap(worksheet);

        var cihazAdiColumn = ResolveRequiredColumn(headers, "Cihaz Adı", "cihazadi", "cihazadi");
        var kategoriColumn = ResolveRequiredColumn(headers, "Kategori", "kategori", "category");
        var markaColumn = ResolveRequiredColumn(headers, "Marka", "marka", "brand");
        var modelColumn = ResolveRequiredColumn(headers, "Model", "model");
        var seriNoColumn = ResolveRequiredColumn(headers, "Seri No", "serino", "seri");
        var envanterNoColumn = ResolveRequiredColumn(headers, "Envanter No", "envanterno", "inventoryno");
        var barkodNoColumn = ResolveOptionalColumn(headers, "Barkod No", "barkodno", "barcode");
        var durumColumn = ResolveRequiredColumn(headers, "Durum", "durum", "status");

        var rows = new List<DeviceImportRowInput>();
        var lastRowNumber = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var rowNumber = 2; rowNumber <= lastRowNumber; rowNumber++)
        {
            var row = worksheet.Row(rowNumber);
            if (IsEmpty(row))
            {
                continue;
            }

            rows.Add(new DeviceImportRowInput(
                rowNumber,
                ReadString(row, cihazAdiColumn),
                ReadString(row, kategoriColumn),
                ReadString(row, markaColumn),
                ReadString(row, modelColumn),
                ReadString(row, seriNoColumn),
                ReadString(row, envanterNoColumn),
                barkodNoColumn.HasValue ? ReadNullableString(row, barkodNoColumn.Value) : null,
                ReadString(row, durumColumn)));
        }

        EnsureAtLeastOneRow(rows.Count);
        return rows;
    }

    public static IReadOnlyList<PersonnelImportRowInput> ParsePersonnelRows(byte[] content)
    {
        var worksheet = GetWorksheet(content);
        var headers = BuildHeaderMap(worksheet);

        var sicilNoColumn = ResolveRequiredColumn(headers, "Sicil No", "sicilno", "registrationno");
        var adColumn = ResolveRequiredColumn(headers, "Ad", "ad", "isim", "name");
        var soyadColumn = ResolveRequiredColumn(headers, "Soyad", "soyad", "surname", "lastname");
        var departmanColumn = ResolveRequiredColumn(headers, "Departman", "departman", "department");
        var pozisyonColumn = ResolveRequiredColumn(headers, "Pozisyon", "pozisyon", "title");
        var zimmetNoColumn = ResolveOptionalColumn(headers, "Zimmet No", "zimmetno", "assignmentno");
        var aktifMiColumn = ResolveRequiredColumn(headers, "Aktif Mi", "aktifmi", "aktif", "active");

        var rows = new List<PersonnelImportRowInput>();
        var lastRowNumber = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var rowNumber = 2; rowNumber <= lastRowNumber; rowNumber++)
        {
            var row = worksheet.Row(rowNumber);
            if (IsEmpty(row))
            {
                continue;
            }

            rows.Add(new PersonnelImportRowInput(
                rowNumber,
                ReadString(row, sicilNoColumn),
                ReadString(row, adColumn),
                ReadString(row, soyadColumn),
                ReadString(row, departmanColumn),
                ReadString(row, pozisyonColumn),
                zimmetNoColumn.HasValue ? ReadNullableString(row, zimmetNoColumn.Value) : null,
                ReadString(row, aktifMiColumn)));
        }

        EnsureAtLeastOneRow(rows.Count);
        return rows;
    }

    private static IXLWorksheet GetWorksheet(byte[] content)
    {
        if (content.Length == 0)
        {
            throw CreateValidationException("Excel dosyası boş olamaz.");
        }

        try
        {
            using var stream = new MemoryStream(content);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            return worksheet?.CopyTo(new XLWorkbook(), "Sheet1")
                ?? throw CreateValidationException("Excel dosyasında okunabilir bir sayfa bulunamadı.");
        }
        catch (Exception exception) when (exception.GetType().Name == "OpenXMLPackageException")
        {
            throw CreateValidationException("Excel dosyası okunamadı. Lütfen geçerli bir .xlsx dosyası yükleyin.");
        }
        catch (InvalidDataException)
        {
            throw CreateValidationException("Excel dosyası bozuk veya desteklenmeyen bir biçimde.");
        }
        catch (FileFormatException)
        {
            throw CreateValidationException("Excel dosyası bozuk veya desteklenmeyen bir biçimde.");
        }
    }

    private static Dictionary<string, int> BuildHeaderMap(IXLWorksheet worksheet)
    {
        var headerRow = worksheet.FirstRowUsed()
            ?? throw CreateValidationException("Excel dosyasında başlık satırı bulunamadı.");

        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var cell in headerRow.CellsUsed())
        {
            var normalized = NormalizeHeader(cell.GetString());
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                map[normalized] = cell.Address.ColumnNumber;
            }
        }

        if (map.Count == 0)
        {
            throw CreateValidationException("Excel dosyasında geçerli başlık satırı bulunamadı.");
        }

        return map;
    }

    private static int ResolveRequiredColumn(
        IReadOnlyDictionary<string, int> headers,
        string displayName,
        params string[] aliases)
    {
        var column = ResolveOptionalColumn(headers, displayName, aliases);
        if (!column.HasValue)
        {
            throw CreateValidationException($"'{displayName}' sütunu bulunamadı.");
        }

        return column.Value;
    }

    private static int? ResolveOptionalColumn(
        IReadOnlyDictionary<string, int> headers,
        string displayName,
        params string[] aliases)
    {
        var normalizedCandidates = new[]
        {
            displayName
        }.Concat(aliases).Select(NormalizeHeader);

        foreach (var candidate in normalizedCandidates)
        {
            if (headers.TryGetValue(candidate, out var column))
            {
                return column;
            }
        }

        return null;
    }

    private static string ReadString(IXLRow row, int columnNumber) =>
        row.Cell(columnNumber).GetValue<string>().Trim();

    private static string? ReadNullableString(IXLRow row, int columnNumber)
    {
        var value = row.Cell(columnNumber).GetValue<string>().Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool IsEmpty(IXLRow row) =>
        row.CellsUsed().All(cell => string.IsNullOrWhiteSpace(cell.GetValue<string>()));

    private static void EnsureAtLeastOneRow(int rowCount)
    {
        if (rowCount == 0)
        {
            throw CreateValidationException("Excel dosyasında işlenecek veri satırı bulunamadı.");
        }
    }

    private static string NormalizeHeader(string value)
    {
        var cleaned = value.Trim().ToLowerInvariant();
        cleaned = cleaned
            .Replace("ı", "i", StringComparison.Ordinal)
            .Replace("İ", "i", StringComparison.Ordinal)
            .Replace("ş", "s", StringComparison.Ordinal)
            .Replace("ğ", "g", StringComparison.Ordinal)
            .Replace("ü", "u", StringComparison.Ordinal)
            .Replace("ö", "o", StringComparison.Ordinal)
            .Replace("ç", "c", StringComparison.Ordinal);

        return string.Concat(cleaned.Where(char.IsLetterOrDigit));
    }

    private static ValidationException CreateValidationException(string message) =>
        new([new ValidationFailure("Excel", message)]);
}

internal static class ExcelImportAnalyzer
{
    public static IReadOnlyList<ImportPlan> AnalyzeDevices(
        IReadOnlyList<DeviceImportRowInput> rows,
        IReadOnlyList<Device> existingDevices,
        IReadOnlyList<DeviceCategory> categories)
    {
        var initialExistingIds = existingDevices.Select(device => device.Id).ToHashSet();
        var states = existingDevices.ToDictionary(
            device => device.Id,
            device => new DeviceState(
                device.Id,
                device.CihazAdi,
                device.SeriNo,
                device.EnvanterNo,
                device.BarkodNo,
                device.Marka,
                device.Model,
                device.CategoryId,
                device.Category.Name,
                device.Status,
                device.PersonelId));

        var categoriesByName = categories.ToDictionary(
            category => NormalizeLookup(category.Name),
            category => category);

        var serialOwners = existingDevices
            .ToDictionary(device => device.SeriNo, device => device.Id, StringComparer.OrdinalIgnoreCase);
        var inventoryOwners = existingDevices
            .ToDictionary(device => device.EnvanterNo, device => device.Id, StringComparer.OrdinalIgnoreCase);
        var barcodeOwners = existingDevices
            .Where(device => !string.IsNullOrWhiteSpace(device.BarkodNo))
            .ToDictionary(device => device.BarkodNo!, device => device.Id, StringComparer.OrdinalIgnoreCase);

        var processedExistingIds = new HashSet<Guid>();
        var plans = new List<ImportPlan>();

        foreach (var row in rows)
        {
            var errors = new List<string>();
            var cihazAdi = NormalizeRequiredText(row.CihazAdi, "Cihaz Adı", 150, errors);
            var kategori = row.Kategori.Trim();
            var marka = NormalizeRequiredText(row.Marka, "Marka", 100, errors);
            var model = NormalizeRequiredText(row.Model, "Model", 100, errors);
            var seriNo = NormalizeRequiredText(row.SeriNo, "Seri No", 100, errors, uppercase: true);
            var envanterNo = NormalizeRequiredText(row.EnvanterNo, "Envanter No", 100, errors, uppercase: true);
            var barkodNo = NormalizeOptionalText(row.BarkodNo, "Barkod No", 100, errors, uppercase: true);

            if (!categoriesByName.TryGetValue(NormalizeLookup(kategori), out var category))
            {
                errors.Add($"Kategori bulunamadı: '{kategori}'.");
            }

            if (!TryParseDeviceStatus(row.Durum, out var status))
            {
                errors.Add($"Geçersiz cihaz durumu: '{row.Durum}'.");
            }

            var serialOwner = ResolveOwner(serialOwners, seriNo, initialExistingIds, "Seri No", errors);
            var inventoryOwner = ResolveOwner(inventoryOwners, envanterNo, initialExistingIds, "Envanter No", errors);
            ResolveOwner(barcodeOwners, barkodNo, initialExistingIds, "Barkod No", errors);

            Guid? existingId = null;
            if (serialOwner.HasValue && inventoryOwner.HasValue && serialOwner != inventoryOwner)
            {
                errors.Add("Seri No ile Envanter No farklı cihazlara ait görünüyor.");
            }
            else
            {
                existingId = serialOwner ?? inventoryOwner;
            }

            if (existingId.HasValue && processedExistingIds.Contains(existingId.Value))
            {
                errors.Add("Aynı cihaz dosya içinde birden fazla satırda hedefleniyor.");
            }

            if (errors.Count > 0)
            {
                plans.Add(ImportPlan.Error(row.RowNumber, $"{seriNo} / {envanterNo}", errors));
                continue;
            }

            if (existingId.HasValue)
            {
                var state = states[existingId.Value];
                ReleaseOwners(state, serialOwners, inventoryOwners, barcodeOwners);

                if (IsReservedByAnother(serialOwners, seriNo, existingId.Value))
                {
                    errors.Add("Bu seri numarası başka bir aktif cihaz tarafından kullanılıyor.");
                }

                if (IsReservedByAnother(inventoryOwners, envanterNo, existingId.Value))
                {
                    errors.Add("Bu envanter numarası başka bir aktif cihaz tarafından kullanılıyor.");
                }

                if (IsReservedByAnother(barcodeOwners, barkodNo, existingId.Value))
                {
                    errors.Add("Bu barkod numarası başka bir aktif cihaz tarafından kullanılıyor.");
                }

                if (state.PersonelId.HasValue && status != DeviceStatus.Assigned)
                {
                    errors.Add("Zimmetli bir cihaz, import ile zimmet dışı duruma geçirilemez.");
                }

                if (!state.PersonelId.HasValue && status == DeviceStatus.Assigned)
                {
                    errors.Add("Zimmetli durumu yalnızca mevcut personel ataması olan cihazlarda kullanılabilir.");
                }

                if (errors.Count > 0)
                {
                    ReserveOwners(state, serialOwners, inventoryOwners, barcodeOwners);
                    plans.Add(ImportPlan.Error(row.RowNumber, $"{seriNo} / {envanterNo}", errors));
                    continue;
                }

                var messages = DescribeDeviceChanges(state, cihazAdi!, marka!, model!, barkodNo, category!, status);
                state.Apply(cihazAdi!, seriNo!, envanterNo!, barkodNo, marka!, model!, category!.Id, category.Name, status);
                ReserveOwners(state, serialOwners, inventoryOwners, barcodeOwners);
                processedExistingIds.Add(existingId.Value);

                plans.Add(messages.Count == 0
                    ? ImportPlan.Unchanged(
                        row.RowNumber,
                        $"{seriNo} / {envanterNo}",
                        "Mevcut cihaz kaydı ile aynı.",
                        existingId.Value,
                        cihazAdi,
                        seriNo,
                        envanterNo,
                        barkodNo,
                        marka,
                        model,
                        category!.Id,
                        status,
                        state.PersonelId)
                    : ImportPlan.Update(
                        row.RowNumber,
                        $"{seriNo} / {envanterNo}",
                        messages,
                        existingId.Value,
                        cihazAdi,
                        seriNo,
                        envanterNo,
                        barkodNo,
                        marka,
                        model,
                        category!.Id,
                        status,
                        state.PersonelId));

                continue;
            }

            if (status == DeviceStatus.Assigned)
            {
                errors.Add("Yeni cihaz importunda zimmetli durum kullanılamaz.");
            }

            if (serialOwners.ContainsKey(seriNo!))
            {
                errors.Add("Bu seri numarası dosya içinde veya sistemde zaten kullanılıyor.");
            }

            if (inventoryOwners.ContainsKey(envanterNo!))
            {
                errors.Add("Bu envanter numarası dosya içinde veya sistemde zaten kullanılıyor.");
            }

            if (!string.IsNullOrWhiteSpace(barkodNo) && barcodeOwners.ContainsKey(barkodNo))
            {
                errors.Add("Bu barkod numarası dosya içinde veya sistemde zaten kullanılıyor.");
            }

            if (errors.Count > 0)
            {
                plans.Add(ImportPlan.Error(row.RowNumber, $"{seriNo} / {envanterNo}", errors));
                continue;
            }

            var tempId = Guid.NewGuid();
            var newState = new DeviceState(
                tempId,
                cihazAdi!,
                seriNo!,
                envanterNo!,
                barkodNo,
                marka!,
                model!,
                category!.Id,
                category.Name,
                status,
                null);

            states[tempId] = newState;
            ReserveOwners(newState, serialOwners, inventoryOwners, barcodeOwners);

            plans.Add(ImportPlan.New(
                row.RowNumber,
                $"{seriNo} / {envanterNo}",
                ["Yeni cihaz kaydı eklenecek."],
                cihazAdi,
                seriNo,
                envanterNo,
                barkodNo,
                marka,
                model,
                category.Id,
                status));
        }

        return plans;
    }

    public static IReadOnlyList<ImportPlan> AnalyzePersonnel(
        IReadOnlyList<PersonnelImportRowInput> rows,
        IReadOnlyList<Personnel> existingPersonnel)
    {
        var initialExistingIds = existingPersonnel.Select(item => item.Id).ToHashSet();
        var states = existingPersonnel.ToDictionary(
            item => item.Id,
            item => new PersonnelState(
                item.Id,
                item.SicilNo,
                item.Ad,
                item.Soyad,
                item.Departman,
                item.Pozisyon,
                item.ZimmetNo,
                item.AktifMi));

        var sicilOwners = existingPersonnel
            .ToDictionary(item => item.SicilNo, item => item.Id, StringComparer.OrdinalIgnoreCase);

        var processedExistingIds = new HashSet<Guid>();
        var plans = new List<ImportPlan>();

        foreach (var row in rows)
        {
            var errors = new List<string>();
            var sicilNo = NormalizeRequiredText(row.SicilNo, "Sicil No", 50, errors, uppercase: true);
            var ad = NormalizeRequiredText(row.Ad, "Ad", 100, errors);
            var soyad = NormalizeRequiredText(row.Soyad, "Soyad", 100, errors);
            var departman = NormalizeRequiredText(row.Departman, "Departman", 120, errors);
            var pozisyon = NormalizeRequiredText(row.Pozisyon, "Pozisyon", 120, errors);
            var zimmetNo = NormalizeOptionalText(row.ZimmetNo, "Zimmet No", 50, errors);

            if (!TryParseBoolean(row.AktifMi, out var aktifMi))
            {
                errors.Add($"Aktif Mi değeri çözümlenemedi: '{row.AktifMi}'.");
            }

            var ownerId = ResolveOwner(sicilOwners, sicilNo, initialExistingIds, "Sicil No", errors);

            if (ownerId.HasValue && processedExistingIds.Contains(ownerId.Value))
            {
                errors.Add("Aynı personel dosya içinde birden fazla satırda hedefleniyor.");
            }

            if (errors.Count > 0)
            {
                plans.Add(ImportPlan.Error(row.RowNumber, sicilNo ?? row.SicilNo, errors));
                continue;
            }

            if (ownerId.HasValue)
            {
                var state = states[ownerId.Value];
                sicilOwners.Remove(state.SicilNo);

                if (IsReservedByAnother(sicilOwners, sicilNo, ownerId.Value))
                {
                    errors.Add("Bu sicil numarası başka bir aktif personel tarafından kullanılıyor.");
                }

                if (errors.Count > 0)
                {
                    sicilOwners[state.SicilNo] = state.Id;
                    plans.Add(ImportPlan.Error(row.RowNumber, sicilNo ?? row.SicilNo, errors));
                    continue;
                }

                var messages = DescribePersonnelChanges(state, ad!, soyad!, departman!, pozisyon!, zimmetNo, aktifMi);
                state.Apply(sicilNo!, ad!, soyad!, departman!, pozisyon!, zimmetNo, aktifMi);
                sicilOwners[state.SicilNo] = state.Id;
                processedExistingIds.Add(ownerId.Value);

                plans.Add(messages.Count == 0
                    ? ImportPlan.Unchanged(
                        row.RowNumber,
                        sicilNo!,
                        "Mevcut personel kaydı ile aynı.",
                        ownerId.Value,
                        sicilNo: sicilNo,
                        ad: ad,
                        soyad: soyad,
                        departman: departman,
                        pozisyon: pozisyon,
                        zimmetNo: zimmetNo,
                        aktifMi: aktifMi)
                    : ImportPlan.Update(
                        row.RowNumber,
                        sicilNo!,
                        messages,
                        ownerId.Value,
                        sicilNo: sicilNo,
                        ad: ad,
                        soyad: soyad,
                        departman: departman,
                        pozisyon: pozisyon,
                        zimmetNo: zimmetNo,
                        aktifMi: aktifMi));

                continue;
            }

            if (sicilOwners.ContainsKey(sicilNo!))
            {
                errors.Add("Bu sicil numarası dosya içinde veya sistemde zaten kullanılıyor.");
            }

            if (errors.Count > 0)
            {
                plans.Add(ImportPlan.Error(row.RowNumber, sicilNo ?? row.SicilNo, errors));
                continue;
            }

            var tempId = Guid.NewGuid();
            var newState = new PersonnelState(
                tempId,
                sicilNo!,
                ad!,
                soyad!,
                departman!,
                pozisyon!,
                zimmetNo,
                aktifMi);

            states[tempId] = newState;
            sicilOwners[newState.SicilNo] = newState.Id;

            plans.Add(ImportPlan.New(
                row.RowNumber,
                sicilNo!,
                ["Yeni personel kaydı eklenecek."],
                sicilNo: sicilNo,
                ad: ad,
                soyad: soyad,
                departman: departman,
                pozisyon: pozisyon,
                zimmetNo: zimmetNo,
                aktifMi: aktifMi));
        }

        return plans;
    }

    public static ImportPreviewSummaryDto BuildPreviewSummary(IReadOnlyList<ImportPlan> plans) =>
        new(
            plans.Count(plan => plan.Action == ImportPlanAction.New),
            plans.Count(plan => plan.Action == ImportPlanAction.Update),
            plans.Count(plan => plan.Action == ImportPlanAction.Unchanged),
            plans.Count(plan => plan.Action == ImportPlanAction.Error),
            plans.Select(plan => plan.ToDto()).ToArray());

    public static ImportExecutionResultDto BuildExecutionResult(IReadOnlyList<ImportPlan> plans) =>
        new(
            plans.Count(plan => plan.Action == ImportPlanAction.New),
            plans.Count(plan => plan.Action == ImportPlanAction.Update),
            plans.Count(plan => plan.Action == ImportPlanAction.Unchanged),
            plans.Count(plan => plan.Action == ImportPlanAction.Error),
            plans.Select(plan => plan.ToDto()).ToArray());

    public static string GetDeviceStatusDisplayName(DeviceStatus status) =>
        status switch
        {
            DeviceStatus.InStock => "Stokta",
            DeviceStatus.Assigned => "Zimmetli",
            DeviceStatus.InService => "Serviste",
            DeviceStatus.Scrap => "Hurda / İmha",
            DeviceStatus.Lost => "Kayıp",
            DeviceStatus.Passive => "Pasif",
            _ => status.ToString()
        };

    private static List<string> DescribeDeviceChanges(
        DeviceState state,
        string cihazAdi,
        string marka,
        string model,
        string? barkodNo,
        DeviceCategory category,
        DeviceStatus status)
    {
        var messages = new List<string>();

        AddChange(messages, "Cihaz Adı", state.CihazAdi, cihazAdi);
        AddChange(messages, "Kategori", state.CategoryName, category.Name);
        AddChange(messages, "Marka", state.Marka, marka);
        AddChange(messages, "Model", state.Model, model);
        AddChange(messages, "Barkod No", state.BarkodNo, barkodNo);
        AddChange(messages, "Durum", GetDeviceStatusDisplayName(state.Status), GetDeviceStatusDisplayName(status));

        return messages;
    }

    private static List<string> DescribePersonnelChanges(
        PersonnelState state,
        string ad,
        string soyad,
        string departman,
        string pozisyon,
        string? zimmetNo,
        bool aktifMi)
    {
        var messages = new List<string>();

        AddChange(messages, "Ad", state.Ad, ad);
        AddChange(messages, "Soyad", state.Soyad, soyad);
        AddChange(messages, "Departman", state.Departman, departman);
        AddChange(messages, "Pozisyon", state.Pozisyon, pozisyon);
        AddChange(messages, "Zimmet No", state.ZimmetNo, zimmetNo);
        AddChange(messages, "Aktif Mi", state.AktifMi ? "Evet" : "Hayır", aktifMi ? "Evet" : "Hayır");

        return messages;
    }

    private static void AddChange(List<string> messages, string field, string? currentValue, string? newValue)
    {
        var left = currentValue ?? "(boş)";
        var right = newValue ?? "(boş)";

        if (!string.Equals(left, right, StringComparison.Ordinal))
        {
            messages.Add($"{field}: '{left}' -> '{right}'");
        }
    }

    private static string? NormalizeRequiredText(
        string value,
        string fieldName,
        int maxLength,
        List<string> errors,
        bool uppercase = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fieldName} zorunludur.");
            return null;
        }

        return NormalizeOptionalText(value, fieldName, maxLength, errors, uppercase);
    }

    private static string? NormalizeOptionalText(
        string? value,
        string fieldName,
        int maxLength,
        List<string> errors,
        bool uppercase = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        normalized = uppercase ? normalized.ToUpperInvariant() : normalized;

        if (normalized.Length > maxLength)
        {
            errors.Add($"{fieldName} en fazla {maxLength} karakter olabilir.");
        }

        return normalized;
    }

    private static Guid? ResolveOwner(
        IReadOnlyDictionary<string, Guid> owners,
        string? key,
        IReadOnlySet<Guid> initialExistingIds,
        string fieldName,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(key) || !owners.TryGetValue(key, out var ownerId))
        {
            return null;
        }

        if (!initialExistingIds.Contains(ownerId))
        {
            errors.Add($"{fieldName} değeri Excel dosyasında tekrar ediyor.");
            return null;
        }

        return ownerId;
    }

    private static bool IsReservedByAnother(
        IReadOnlyDictionary<string, Guid> owners,
        string? key,
        Guid currentId) =>
        !string.IsNullOrWhiteSpace(key)
        && owners.TryGetValue(key, out var ownerId)
        && ownerId != currentId;

    private static bool TryParseDeviceStatus(string value, out DeviceStatus status)
    {
        switch (NormalizeLookup(value))
        {
            case "1":
            case "stokta":
            case "stok":
            case "instock":
                status = DeviceStatus.InStock;
                return true;
            case "2":
            case "zimmetli":
            case "assigned":
                status = DeviceStatus.Assigned;
                return true;
            case "3":
            case "serviste":
            case "servis":
            case "inservice":
                status = DeviceStatus.InService;
                return true;
            case "4":
            case "hurda":
            case "imha":
            case "hurdaimha":
            case "scrap":
                status = DeviceStatus.Scrap;
                return true;
            case "5":
            case "kayip":
            case "kayıp":
            case "lost":
                status = DeviceStatus.Lost;
                return true;
            case "6":
            case "pasif":
            case "passive":
                status = DeviceStatus.Passive;
                return true;
            default:
                status = default;
                return false;
        }
    }

    private static bool TryParseBoolean(string value, out bool result)
    {
        switch (NormalizeLookup(value))
        {
            case "1":
            case "true":
            case "evet":
            case "yes":
            case "aktif":
                result = true;
                return true;
            case "0":
            case "false":
            case "hayir":
            case "hayır":
            case "no":
            case "pasif":
                result = false;
                return true;
            default:
                result = false;
                return false;
        }
    }

    private static void ReleaseOwners(
        DeviceState state,
        IDictionary<string, Guid> serialOwners,
        IDictionary<string, Guid> inventoryOwners,
        IDictionary<string, Guid> barcodeOwners)
    {
        serialOwners.Remove(state.SeriNo);
        inventoryOwners.Remove(state.EnvanterNo);

        if (!string.IsNullOrWhiteSpace(state.BarkodNo))
        {
            barcodeOwners.Remove(state.BarkodNo);
        }
    }

    private static void ReserveOwners(
        DeviceState state,
        IDictionary<string, Guid> serialOwners,
        IDictionary<string, Guid> inventoryOwners,
        IDictionary<string, Guid> barcodeOwners)
    {
        serialOwners[state.SeriNo] = state.Id;
        inventoryOwners[state.EnvanterNo] = state.Id;

        if (!string.IsNullOrWhiteSpace(state.BarkodNo))
        {
            barcodeOwners[state.BarkodNo] = state.Id;
        }
    }

    private static string NormalizeLookup(string value)
    {
        var lowered = value.Trim().ToLowerInvariant();
        lowered = lowered
            .Replace("ı", "i", StringComparison.Ordinal)
            .Replace("ş", "s", StringComparison.Ordinal)
            .Replace("ğ", "g", StringComparison.Ordinal)
            .Replace("ü", "u", StringComparison.Ordinal)
            .Replace("ö", "o", StringComparison.Ordinal)
            .Replace("ç", "c", StringComparison.Ordinal);

        return string.Concat(lowered.Where(char.IsLetterOrDigit));
    }
}

internal sealed record DeviceImportRowInput(
    int RowNumber,
    string CihazAdi,
    string Kategori,
    string Marka,
    string Model,
    string SeriNo,
    string EnvanterNo,
    string? BarkodNo,
    string Durum);

internal sealed record PersonnelImportRowInput(
    int RowNumber,
    string SicilNo,
    string Ad,
    string Soyad,
    string Departman,
    string Pozisyon,
    string? ZimmetNo,
    string AktifMi);

internal enum ImportPlanAction
{
    New,
    Update,
    Unchanged,
    Error
}

internal sealed record ImportPlan(
    int RowNumber,
    ImportPlanAction Action,
    string Identifier,
    string Summary,
    IReadOnlyList<string> Messages,
    Guid? ExistingEntityId = null,
    string? CihazAdi = null,
    string? SeriNo = null,
    string? EnvanterNo = null,
    string? BarkodNo = null,
    string? Marka = null,
    string? Model = null,
    Guid? CategoryId = null,
    DeviceStatus? DeviceStatus = null,
    Guid? PersonelId = null,
    string? SicilNo = null,
    string? Ad = null,
    string? Soyad = null,
    string? Departman = null,
    string? Pozisyon = null,
    string? ZimmetNo = null,
    bool? AktifMi = null)
{
    public static ImportPlan Error(int rowNumber, string identifier, IReadOnlyList<string> messages) =>
        new(rowNumber, ImportPlanAction.Error, identifier, "Satır işlenemedi.", messages);

    public static ImportPlan New(
        int rowNumber,
        string identifier,
        IReadOnlyList<string> messages,
        string? cihazAdi = null,
        string? seriNo = null,
        string? envanterNo = null,
        string? barkodNo = null,
        string? marka = null,
        string? model = null,
        Guid? categoryId = null,
        DeviceStatus? deviceStatus = null,
        Guid? personelId = null,
        string? sicilNo = null,
        string? ad = null,
        string? soyad = null,
        string? departman = null,
        string? pozisyon = null,
        string? zimmetNo = null,
        bool? aktifMi = null) =>
        new(
            rowNumber,
            ImportPlanAction.New,
            identifier,
            "Yeni kayıt oluşturulacak.",
            messages,
            CihazAdi: cihazAdi,
            SeriNo: seriNo,
            EnvanterNo: envanterNo,
            BarkodNo: barkodNo,
            Marka: marka,
            Model: model,
            CategoryId: categoryId,
            DeviceStatus: deviceStatus,
            PersonelId: personelId,
            SicilNo: sicilNo,
            Ad: ad,
            Soyad: soyad,
            Departman: departman,
            Pozisyon: pozisyon,
            ZimmetNo: zimmetNo,
            AktifMi: aktifMi);

    public static ImportPlan Update(
        int rowNumber,
        string identifier,
        IReadOnlyList<string> messages,
        Guid existingId,
        string? cihazAdi = null,
        string? seriNo = null,
        string? envanterNo = null,
        string? barkodNo = null,
        string? marka = null,
        string? model = null,
        Guid? categoryId = null,
        DeviceStatus? deviceStatus = null,
        Guid? personelId = null,
        string? sicilNo = null,
        string? ad = null,
        string? soyad = null,
        string? departman = null,
        string? pozisyon = null,
        string? zimmetNo = null,
        bool? aktifMi = null) =>
        new(
            rowNumber,
            ImportPlanAction.Update,
            identifier,
            "Mevcut kayıt güncellenecek.",
            messages,
            existingId,
            cihazAdi,
            seriNo,
            envanterNo,
            barkodNo,
            marka,
            model,
            categoryId,
            deviceStatus,
            personelId,
            sicilNo,
            ad,
            soyad,
            departman,
            pozisyon,
            zimmetNo,
            aktifMi);

    public static ImportPlan Unchanged(
        int rowNumber,
        string identifier,
        string summary,
        Guid existingId,
        string? cihazAdi = null,
        string? seriNo = null,
        string? envanterNo = null,
        string? barkodNo = null,
        string? marka = null,
        string? model = null,
        Guid? categoryId = null,
        DeviceStatus? deviceStatus = null,
        Guid? personelId = null,
        string? sicilNo = null,
        string? ad = null,
        string? soyad = null,
        string? departman = null,
        string? pozisyon = null,
        string? zimmetNo = null,
        bool? aktifMi = null) =>
        new(
            rowNumber,
            ImportPlanAction.Unchanged,
            identifier,
            summary,
            ["Değişiklik tespit edilmedi."],
            existingId,
            cihazAdi,
            seriNo,
            envanterNo,
            barkodNo,
            marka,
            model,
            categoryId,
            deviceStatus,
            personelId,
            sicilNo,
            ad,
            soyad,
            departman,
            pozisyon,
            zimmetNo,
            aktifMi);

    public ImportPreviewRowDto ToDto() =>
        new(
            RowNumber,
            Action switch
            {
                ImportPlanAction.New => "new",
                ImportPlanAction.Update => "update",
                ImportPlanAction.Unchanged => "unchanged",
                _ => "error"
            },
            Identifier,
            Summary,
            Messages);
}

internal sealed class DeviceState(
    Guid id,
    string cihazAdi,
    string seriNo,
    string envanterNo,
    string? barkodNo,
    string marka,
    string model,
    Guid categoryId,
    string categoryName,
    DeviceStatus status,
    Guid? personelId)
{
    public Guid Id { get; } = id;
    public string CihazAdi { get; private set; } = cihazAdi;
    public string SeriNo { get; private set; } = seriNo;
    public string EnvanterNo { get; private set; } = envanterNo;
    public string? BarkodNo { get; private set; } = barkodNo;
    public string Marka { get; private set; } = marka;
    public string Model { get; private set; } = model;
    public Guid CategoryId { get; private set; } = categoryId;
    public string CategoryName { get; private set; } = categoryName;
    public DeviceStatus Status { get; private set; } = status;
    public Guid? PersonelId { get; } = personelId;

    public void Apply(
        string cihazAdi,
        string seriNo,
        string envanterNo,
        string? barkodNo,
        string marka,
        string model,
        Guid categoryId,
        string categoryName,
        DeviceStatus status)
    {
        CihazAdi = cihazAdi;
        SeriNo = seriNo;
        EnvanterNo = envanterNo;
        BarkodNo = barkodNo;
        Marka = marka;
        Model = model;
        CategoryId = categoryId;
        CategoryName = categoryName;
        Status = status;
    }
}

internal sealed class PersonnelState(
    Guid id,
    string sicilNo,
    string ad,
    string soyad,
    string departman,
    string pozisyon,
    string? zimmetNo,
    bool aktifMi)
{
    public Guid Id { get; } = id;
    public string SicilNo { get; private set; } = sicilNo;
    public string Ad { get; private set; } = ad;
    public string Soyad { get; private set; } = soyad;
    public string Departman { get; private set; } = departman;
    public string Pozisyon { get; private set; } = pozisyon;
    public string? ZimmetNo { get; private set; } = zimmetNo;
    public bool AktifMi { get; private set; } = aktifMi;

    public void Apply(
        string sicilNo,
        string ad,
        string soyad,
        string departman,
        string pozisyon,
        string? zimmetNo,
        bool aktifMi)
    {
        SicilNo = sicilNo;
        Ad = ad;
        Soyad = soyad;
        Departman = departman;
        Pozisyon = pozisyon;
        ZimmetNo = zimmetNo;
        AktifMi = aktifMi;
    }
}
