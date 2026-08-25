using FluentValidation;
using InventorySystem.Application.Queries;

namespace InventorySystem.Application.Validators;

public sealed class GetDeviceByIdQueryValidator : AbstractValidator<GetDeviceByIdQuery>
{
    public GetDeviceByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Cihaz ID boş olamaz.");
    }
}
