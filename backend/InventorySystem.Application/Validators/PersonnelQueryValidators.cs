using FluentValidation;
using InventorySystem.Application.Queries;

namespace InventorySystem.Application.Validators;

public sealed class GetPersonnelByIdQueryValidator : AbstractValidator<GetPersonnelByIdQuery>
{
    public GetPersonnelByIdQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
