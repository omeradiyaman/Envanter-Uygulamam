using FluentValidation;
using InventorySystem.Application.Commands;

namespace InventorySystem.Application.Validators;

public sealed class CreatePersonnelCommandValidator : AbstractValidator<CreatePersonnelCommand>
{
    public CreatePersonnelCommandValidator()
    {
        Include(new PersonnelFieldsValidator());
    }

    private sealed class PersonnelFieldsValidator : AbstractValidator<CreatePersonnelCommand>
    {
        public PersonnelFieldsValidator()
        {
            RuleFor(command => command.SicilNo).NotEmpty().MaximumLength(50);
            RuleFor(command => command.Ad).NotEmpty().MaximumLength(100);
            RuleFor(command => command.Soyad).NotEmpty().MaximumLength(100);
            RuleFor(command => command.Departman).NotEmpty().MaximumLength(120);
            RuleFor(command => command.Pozisyon).NotEmpty().MaximumLength(120);
            RuleFor(command => command.ZimmetNo).MaximumLength(50);
        }
    }
}

public sealed class UpdatePersonnelCommandValidator : AbstractValidator<UpdatePersonnelCommand>
{
    public UpdatePersonnelCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.SicilNo).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Ad).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Soyad).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Departman).NotEmpty().MaximumLength(120);
        RuleFor(command => command.Pozisyon).NotEmpty().MaximumLength(120);
        RuleFor(command => command.ZimmetNo).MaximumLength(50);
    }
}

public sealed class DeletePersonnelCommandValidator : AbstractValidator<DeletePersonnelCommand>
{
    public DeletePersonnelCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
