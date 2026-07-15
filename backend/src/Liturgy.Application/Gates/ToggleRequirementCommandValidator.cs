using FluentValidation;

namespace Liturgy.Application.Gates;

public class ToggleRequirementCommandValidator : AbstractValidator<ToggleRequirementCommand>
{
    public ToggleRequirementCommandValidator()
    {
        RuleFor(x => x.RequirementId).NotEmpty();
    }
}
