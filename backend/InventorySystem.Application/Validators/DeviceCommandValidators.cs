using FluentValidation;
using InventorySystem.Application.Commands;
using InventorySystem.Application.Interfaces;

namespace InventorySystem.Application.Validators;

public sealed class CreateDeviceCommandValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceCommandValidator(IDeviceRepository deviceRepository, IPersonnelRepository personnelRepository)
    {
        RuleFor(v => v.CihazAdi)
            .NotEmpty().WithMessage("Cihaz Adı boş olamaz.")
            .MaximumLength(150).WithMessage("Cihaz Adı 150 karakterden uzun olamaz.");

        RuleFor(v => v.SeriNo)
            .NotEmpty().WithMessage("Seri No boş olamaz.")
            .MaximumLength(100).WithMessage("Seri No 100 karakterden uzun olamaz.")
            .MustAsync(async (seriNo, cancellationToken) => 
                !await deviceRepository.SerialNumberExistsAsync(seriNo, null, cancellationToken))
            .WithMessage("Bu seri numarası zaten sistemde kayıtlı.");

        RuleFor(v => v.EnvanterNo)
            .NotEmpty().WithMessage("Envanter No boş olamaz.")
            .MaximumLength(100).WithMessage("Envanter No 100 karakterden uzun olamaz.")
            .MustAsync(async (envanterNo, cancellationToken) => 
                !await deviceRepository.InventoryNumberExistsAsync(envanterNo, null, cancellationToken))
            .WithMessage("Bu envanter numarası zaten sistemde kayıtlı.");

        RuleFor(v => v.BarkodNo)
            .MaximumLength(100).WithMessage("Barkod No 100 karakterden uzun olamaz.");

        RuleFor(v => v.Marka)
            .NotEmpty().WithMessage("Marka boş olamaz.")
            .MaximumLength(100).WithMessage("Marka 100 karakterden uzun olamaz.");

        RuleFor(v => v.Model)
            .NotEmpty().WithMessage("Model boş olamaz.")
            .MaximumLength(100).WithMessage("Model 100 karakterden uzun olamaz.");

        RuleFor(v => v.CategoryId)
            .NotEmpty().WithMessage("Kategori seçimi zorunludur.")
            .MustAsync(async (categoryId, cancellationToken) => 
                await deviceRepository.CategoryExistsAsync(categoryId, cancellationToken))
            .WithMessage("Geçersiz kategori.");

        RuleFor(v => v.Status)
            .IsInEnum().WithMessage("Geçersiz cihaz durumu.");
            
        RuleFor(v => v.PersonelId)
            .MustAsync(async (personelId, cancellationToken) => 
            {
                if (!personelId.HasValue) return true;
                return await personnelRepository.GetByIdAsync(personelId.Value, cancellationToken) != null;
            })
            .WithMessage("Geçersiz personel.")
            .When(v => v.PersonelId.HasValue);

        CreateDeviceWarrantyRules.AddWarrantyRules(this);
    }
}

public sealed class UpdateDeviceCommandValidator : AbstractValidator<UpdateDeviceCommand>
{
    public UpdateDeviceCommandValidator(IDeviceRepository deviceRepository, IPersonnelRepository personnelRepository)
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id boş olamaz.");

        RuleFor(v => v.CihazAdi)
            .NotEmpty().WithMessage("Cihaz Adı boş olamaz.")
            .MaximumLength(150).WithMessage("Cihaz Adı 150 karakterden uzun olamaz.");

        RuleFor(v => v.SeriNo)
            .NotEmpty().WithMessage("Seri No boş olamaz.")
            .MaximumLength(100).WithMessage("Seri No 100 karakterden uzun olamaz.")
            .MustAsync(async (command, seriNo, cancellationToken) => 
                !await deviceRepository.SerialNumberExistsAsync(seriNo, command.Id, cancellationToken))
            .WithMessage("Bu seri numarası zaten sistemde kayıtlı.");

        RuleFor(v => v.EnvanterNo)
            .NotEmpty().WithMessage("Envanter No boş olamaz.")
            .MaximumLength(100).WithMessage("Envanter No 100 karakterden uzun olamaz.")
            .MustAsync(async (command, envanterNo, cancellationToken) => 
                !await deviceRepository.InventoryNumberExistsAsync(envanterNo, command.Id, cancellationToken))
            .WithMessage("Bu envanter numarası zaten sistemde kayıtlı.");

        RuleFor(v => v.BarkodNo)
            .MaximumLength(100).WithMessage("Barkod No 100 karakterden uzun olamaz.");

        RuleFor(v => v.Marka)
            .NotEmpty().WithMessage("Marka boş olamaz.")
            .MaximumLength(100).WithMessage("Marka 100 karakterden uzun olamaz.");

        RuleFor(v => v.Model)
            .NotEmpty().WithMessage("Model boş olamaz.")
            .MaximumLength(100).WithMessage("Model 100 karakterden uzun olamaz.");

        RuleFor(v => v.CategoryId)
            .NotEmpty().WithMessage("Kategori seçimi zorunludur.")
            .MustAsync(async (categoryId, cancellationToken) => 
                await deviceRepository.CategoryExistsAsync(categoryId, cancellationToken))
            .WithMessage("Geçersiz kategori.");

        RuleFor(v => v.Status)
            .IsInEnum().WithMessage("Geçersiz cihaz durumu.");
            
        RuleFor(v => v.PersonelId)
            .MustAsync(async (personelId, cancellationToken) => 
            {
                if (!personelId.HasValue) return true;
                return await personnelRepository.GetByIdAsync(personelId.Value, cancellationToken) != null;
            })
            .WithMessage("Geçersiz personel.")
            .When(v => v.PersonelId.HasValue);

        AddWarrantyRules(this);
    }

    private static void AddWarrantyRules(AbstractValidator<UpdateDeviceCommand> validator)
    {
        validator.RuleFor(v => v.WarrantyProvider).MaximumLength(200).WithMessage("Garanti firması 200 karakterden uzun olamaz.");
        validator.RuleFor(v => v.WarrantyNote).MaximumLength(1000).WithMessage("Garanti notu 1000 karakterden uzun olamaz.");
        validator.RuleFor(v => v.WarrantyEndDate)
            .GreaterThanOrEqualTo(v => v.WarrantyStartDate)
            .When(v => v.WarrantyStartDate.HasValue && v.WarrantyEndDate.HasValue)
            .WithMessage("Garanti bitiş tarihi başlangıç tarihinden önce olamaz.");
    }
}

file static class CreateDeviceWarrantyRules
{
    public static void AddWarrantyRules(AbstractValidator<CreateDeviceCommand> validator)
    {
        validator.RuleFor(v => v.WarrantyProvider).MaximumLength(200).WithMessage("Garanti firması 200 karakterden uzun olamaz.");
        validator.RuleFor(v => v.WarrantyNote).MaximumLength(1000).WithMessage("Garanti notu 1000 karakterden uzun olamaz.");
        validator.RuleFor(v => v.WarrantyEndDate)
            .GreaterThanOrEqualTo(v => v.WarrantyStartDate)
            .When(v => v.WarrantyStartDate.HasValue && v.WarrantyEndDate.HasValue)
            .WithMessage("Garanti bitiş tarihi başlangıç tarihinden önce olamaz.");
    }
}
