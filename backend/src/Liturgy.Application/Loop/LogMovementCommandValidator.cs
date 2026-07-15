using FluentValidation;

namespace Liturgy.Application.Loop;

public class LogMovementCommandValidator : AbstractValidator<LogMovementCommand>
{
    public LogMovementCommandValidator()
    {
        RuleFor(x => x.CardId).NotEmpty();
    }
}
