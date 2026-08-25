using FluentValidation;
using InventorySystem.Application.Commands;
using InventorySystem.Application.Interfaces;

namespace InventorySystem.Application.Validators;

public sealed class AssignDeviceCommandValidator : AbstractValidator<AssignDeviceCommand>
{
    public AssignDeviceCommandValidator(
        IDeviceRepository deviceRepository,
        IPersonnelRepository personnelRepository)
    {
        RuleFor(v => v.DeviceId)
            .NotEmpty().WithMessage("Cihaz ID boş olamaz.")
            .MustAsync(async (id, ct) =>
                await deviceRepository.GetByIdAsync(id, ct) != null)
            .WithMessage("Belirtilen cihaz bulunamadı veya silinmiş.");

        RuleFor(v => v.PersonnelId)
            .NotEmpty().WithMessage("Personel ID boş olamaz.")
            .MustAsync(async (id, ct) =>
                await personnelRepository.GetByIdAsync(id, ct) != null)
            .WithMessage("Belirtilen personel bulunamadı veya silinmiş.");

        RuleFor(v => v.Note)
            .MaximumLength(500).WithMessage("Not 500 karakterden uzun olamaz.");
    }
}

public sealed class UnassignDeviceCommandValidator : AbstractValidator<UnassignDeviceCommand>
{
    public UnassignDeviceCommandValidator(IDeviceRepository deviceRepository)
    {
        RuleFor(v => v.DeviceId)
            .NotEmpty().WithMessage("Cihaz ID boş olamaz.")
            .MustAsync(async (id, ct) =>
                await deviceRepository.GetByIdAsync(id, ct) != null)
            .WithMessage("Belirtilen cihaz bulunamadı veya silinmiş.");

        RuleFor(v => v.Note)
            .MaximumLength(500).WithMessage("Not 500 karakterden uzun olamaz.");
    }
}
