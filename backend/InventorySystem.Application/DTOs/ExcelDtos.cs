namespace InventorySystem.Application.DTOs;

public sealed record ExcelFileDto(
    string FileName,
    string ContentType,
    byte[] Content);

public sealed record ImportPreviewRowDto(
    int RowNumber,
    string Action,
    string Identifier,
    string Summary,
    IReadOnlyList<string> Messages);

public sealed record ImportPreviewSummaryDto(
    int NewCount,
    int UpdatedCount,
    int UnchangedCount,
    int ErrorCount,
    IReadOnlyList<ImportPreviewRowDto> Rows);

public sealed record ImportExecutionResultDto(
    int AddedCount,
    int UpdatedCount,
    int UnchangedCount,
    int ErrorCount,
    IReadOnlyList<ImportPreviewRowDto> Rows);
