using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InventorySystem.Infrastructure.Services;

public sealed class AssignmentDocumentPdfGenerator : IAssignmentDocumentPdfGenerator
{
    public AssignmentDocumentPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(AssignmentDocumentPdfModel model)
    {
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(32);
            page.DefaultTextStyle(style => style.FontSize(9));

            page.Header().Column(header =>
            {
                header.Item().Text("ZİMMET TESLİM BELGESİ").Bold().FontSize(18).FontColor(Colors.Blue.Darken2);
                header.Item().PaddingTop(3).Text($"Belge tarihi: {FormatDate(model.GeneratedAt)}").FontColor(Colors.Grey.Darken1);
            });

            page.Content().PaddingVertical(18).Column(column =>
            {
                column.Spacing(14);
                column.Item().Background(Colors.Grey.Lighten4).Padding(12).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(90);
                        columns.RelativeColumn();
                        columns.ConstantColumn(90);
                        columns.RelativeColumn();
                    });
                    AddInfoRow(table, "Ad Soyad", model.PersonnelName, "Sicil No", model.SicilNo);
                    AddInfoRow(table, "Departman", model.Department, "Zimmet No", model.AssignmentNumber ?? "-");
                });

                column.Item().Text("Aktif Zimmetli Cihazlar").Bold().FontSize(12);
                if (model.Devices.Count == 0)
                {
                    column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(14)
                        .Text("Personele ait aktif zimmetli cihaz bulunmamaktadır.").FontColor(Colors.Grey.Darken1);
                }
                else
                {
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.6f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.1f);
                        });
                        foreach (var title in new[] { "Cihaz", "Marka / Model", "Seri No", "Envanter No", "Zimmet Tarihi" })
                        {
                            table.Cell().Element(HeaderCell).Text(title).Bold();
                        }
                        foreach (var device in model.Devices)
                        {
                            table.Cell().Element(DataCell).Text(device.DeviceName);
                            table.Cell().Element(DataCell).Text(device.BrandModel);
                            table.Cell().Element(DataCell).Text(device.SerialNumber);
                            table.Cell().Element(DataCell).Text(device.InventoryNumber);
                            table.Cell().Element(DataCell).Text(FormatDate(device.AssignedAt));
                        }
                    });
                }

                column.Item().PaddingTop(16).Row(row =>
                {
                    row.RelativeItem().Column(signature =>
                    {
                        signature.Item().Text("Teslim Eden").Bold();
                        signature.Item().PaddingTop(28).BorderTop(1).BorderColor(Colors.Grey.Medium).Text("Ad Soyad / İmza").FontSize(8);
                    });
                    row.ConstantItem(50);
                    row.RelativeItem().Column(signature =>
                    {
                        signature.Item().Text("Teslim Alan").Bold();
                        signature.Item().PaddingTop(28).BorderTop(1).BorderColor(Colors.Grey.Medium).Text($"{model.PersonnelName} / İmza").FontSize(8);
                    });
                });
            });

            page.Footer()
                .DefaultTextStyle(style => style.FontSize(8).FontColor(Colors.Grey.Medium))
                .AlignCenter().Text(text =>
            {
                text.Span("Sayfa ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        })).GeneratePdf();
    }

    private static void AddInfoRow(TableDescriptor table, string label1, string value1, string label2, string value2)
    {
        table.Cell().Padding(3).Text(label1).SemiBold().FontColor(Colors.Grey.Darken1);
        table.Cell().Padding(3).Text(value1);
        table.Cell().Padding(3).Text(label2).SemiBold().FontColor(Colors.Grey.Darken1);
        table.Cell().Padding(3).Text(value2);
    }

    private static IContainer HeaderCell(IContainer container) =>
        container.Background(Colors.Blue.Darken2).Padding(5).DefaultTextStyle(style => style.FontColor(Colors.White).FontSize(8));

    private static IContainer DataCell(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).DefaultTextStyle(style => style.FontSize(8));

    private static string FormatDate(DateTimeOffset value) => value.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
}
