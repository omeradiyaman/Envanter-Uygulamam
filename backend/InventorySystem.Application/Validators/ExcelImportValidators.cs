using FluentValidation;
using InventorySystem.Application.Commands;

namespace InventorySystem.Application.Validators;

public sealed class PreviewDeviceImportCommandValidator
    : AbstractValidator<PreviewDeviceImportCommand>
{
    public PreviewDeviceImportCommandValidator()
    {
        ApplyRules();
    }

    private void ApplyRules()
    {
        RuleFor(command => command.FileName)
            .NotEmpty()
            .Must(HaveValidExcelExtension)
            .WithMessage("Yalnızca .xlsx uzantılı dosyalar kabul edilir.");

        RuleFor(command => command.Content)
            .NotNull()
            .Must(content => content.Length > 0)
            .WithMessage("Yüklenecek Excel dosyası boş olamaz.")
            .Must(content => content.Length <= ExcelValidationConstants.MaxFileSizeBytes)
            .WithMessage($"Excel dosyası en fazla {ExcelValidationConstants.MaxFileSizeMegabytes} MB olabilir.");
    }

    private static bool HaveValidExcelExtension(string fileName) =>
        Path.GetExtension(fileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase);
}

public sealed class ImportDevicesCommandValidator
    : AbstractValidator<ImportDevicesCommand>
{
    public ImportDevicesCommandValidator()
    {
        Include(new PreviewDeviceImportCommandValidatorAdapter());
    }

    private sealed class PreviewDeviceImportCommandValidatorAdapter
        : AbstractValidator<ImportDevicesCommand>
    {
        public PreviewDeviceImportCommandValidatorAdapter()
        {
            RuleFor(command => command.FileName)
                .NotEmpty()
                .Must(fileName => Path.GetExtension(fileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Yalnızca .xlsx uzantılı dosyalar kabul edilir.");

            RuleFor(command => command.Content)
                .NotNull()
                .Must(content => content.Length > 0)
                .WithMessage("Yüklenecek Excel dosyası boş olamaz.")
                .Must(content => content.Length <= ExcelValidationConstants.MaxFileSizeBytes)
                .WithMessage($"Excel dosyası en fazla {ExcelValidationConstants.MaxFileSizeMegabytes} MB olabilir.");
        }
    }
}

public sealed class PreviewPersonnelImportCommandValidator
    : AbstractValidator<PreviewPersonnelImportCommand>
{
    public PreviewPersonnelImportCommandValidator()
    {
        RuleFor(command => command.FileName)
            .NotEmpty()
            .Must(fileName => Path.GetExtension(fileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Yalnızca .xlsx uzantılı dosyalar kabul edilir.");

        RuleFor(command => command.Content)
            .NotNull()
            .Must(content => content.Length > 0)
            .WithMessage("Yüklenecek Excel dosyası boş olamaz.")
            .Must(content => content.Length <= ExcelValidationConstants.MaxFileSizeBytes)
            .WithMessage($"Excel dosyası en fazla {ExcelValidationConstants.MaxFileSizeMegabytes} MB olabilir.");
    }
}

public sealed class ImportPersonnelCommandValidator
    : AbstractValidator<ImportPersonnelCommand>
{
    public ImportPersonnelCommandValidator()
    {
        RuleFor(command => command.FileName)
            .NotEmpty()
            .Must(fileName => Path.GetExtension(fileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Yalnızca .xlsx uzantılı dosyalar kabul edilir.");

        RuleFor(command => command.Content)
            .NotNull()
            .Must(content => content.Length > 0)
            .WithMessage("Yüklenecek Excel dosyası boş olamaz.")
            .Must(content => content.Length <= ExcelValidationConstants.MaxFileSizeBytes)
            .WithMessage($"Excel dosyası en fazla {ExcelValidationConstants.MaxFileSizeMegabytes} MB olabilir.");
    }
}

internal static class ExcelValidationConstants
{
    public const int MaxFileSizeMegabytes = 5;
    public const int MaxFileSizeBytes = MaxFileSizeMegabytes * 1024 * 1024;
}
