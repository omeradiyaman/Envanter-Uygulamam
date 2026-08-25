using FluentValidation;
using InventorySystem.Application.Commands;

namespace InventorySystem.Application.Validators;

public sealed class CreateDeviceCategoryCommandValidator : AbstractValidator<CreateDeviceCategoryCommand>
{
    public CreateDeviceCategoryCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Description).MaximumLength(500);
    }
}

public sealed class UpdateDeviceCategoryCommandValidator : AbstractValidator<UpdateDeviceCategoryCommand>
{
    public UpdateDeviceCategoryCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Description).MaximumLength(500);
    }
}
