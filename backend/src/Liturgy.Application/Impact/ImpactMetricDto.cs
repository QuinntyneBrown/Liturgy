using Liturgy.Domain;

namespace Liturgy.Application.Impact;

public record ImpactMetricDto(string Value, string? Unit, string Label, bool Highlight)
{
    public static ImpactMetricDto From(ImpactMetric m) => new(m.Value, m.Unit, m.Label, m.Highlight);
}
