namespace Liturgy.Application.Impact;

public record ImpactDto(
    string Headline,
    IReadOnlyList<ImpactMetricDto> Metrics,
    IReadOnlyList<StoryDto> Stories,
    IReadOnlyList<GratitudeDto> Gratitude);
