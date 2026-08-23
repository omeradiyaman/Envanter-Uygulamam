using FluentValidation;
using InventorySystem.Application.Queries;

namespace InventorySystem.Application.Validators;

public sealed class GetSystemStatusQueryValidator : AbstractValidator<GetSystemStatusQuery>
{
    public GetSystemStatusQueryValidator()
    {
        RuleFor(query => query.Client)
            .NotEmpty()
            .MaximumLength(50);
    }
}
