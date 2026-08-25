using FluentValidation;
using InventorySystem.Application.Commands;

namespace InventorySystem.Application.Validators;

public sealed class UploadAssignmentDocumentCommandValidator : AbstractValidator<UploadAssignmentDocumentCommand>
{
    public const long MaximumFileSize = 10 * 1024 * 1024;
    private static readonly string[] AllowedContentTypes = ["application/pdf", "image/png", "image/jpeg"];

    public UploadAssignmentDocumentCommandValidator()
    {
        RuleFor(command => command.PersonnelId).NotEmpty();
        RuleFor(command => command.OriginalFileName).NotEmpty().MaximumLength(255);
        RuleFor(command => command.ContentType).Must(type => AllowedContentTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Yalnızca PDF, PNG ve JPG/JPEG dosyaları yüklenebilir.");
        RuleFor(command => command.FileSize).GreaterThan(0).LessThanOrEqualTo(MaximumFileSize)
            .WithMessage("Dosya boyutu 10 MB'dan büyük olamaz.");
        RuleFor(command => command.Content).Must((command, content) => content.LongLength == command.FileSize)
            .WithMessage("Dosya boyutu doğrulanamadı.");
        RuleFor(command => command.Description).MaximumLength(500);
    }
}
