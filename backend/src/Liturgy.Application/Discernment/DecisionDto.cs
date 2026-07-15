using Liturgy.Domain;

namespace Liturgy.Application.Discernment;

public record DecisionDto(string? ChosenPath, string Rationale, string PrayedOverWith, string? DecidedAt)
{
    public static DecisionDto Empty { get; } = new(null, string.Empty, string.Empty, null);

    public static DecisionDto From(Decision decision) => new(
        decision.ChosenPath.ToString(),
        decision.Rationale,
        decision.PrayedOverWith,
        decision.DecidedAt.ToString("O"));
}
