using FluentValidation;

namespace Liturgy.Application.Projects;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Tag).NotEmpty().MaximumLength(160);
    }
}
