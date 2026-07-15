using FluentValidation;

namespace Liturgy.Application.Board;

public class PointCardCommandValidator : AbstractValidator<PointCardCommand>
{
    public PointCardCommandValidator()
    {
        RuleFor(x => x.Points)
            .InclusiveBetween(0, 999)
            .When(x => x.Points.HasValue);
    }
}
