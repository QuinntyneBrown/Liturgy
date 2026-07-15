using FluentValidation;

namespace Liturgy.Application.Discernment;

public class UpdateDecisionCommandValidator : AbstractValidator<UpdateDecisionCommand>
{
    public UpdateDecisionCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.ChosenPath).IsInEnum();
        RuleFor(x => x.Rationale).MaximumLength(2000);
        RuleFor(x => x.PrayedOverWith).MaximumLength(200);
    }
}
