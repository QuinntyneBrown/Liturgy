using FluentValidation;

namespace Liturgy.Application.Projects;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Tag).NotEmpty().MaximumLength(160);
    }
}
