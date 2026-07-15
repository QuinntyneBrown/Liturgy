// Traces to: L2-IMPACT-001
using System.Net.Http.Json;
using Liturgy.IntegrationTests.Support;
using Xunit;

namespace Liturgy.IntegrationTests;

/// <summary>Proves the Demonstrate impact screen returns the relationship-framed headline plus non-empty metrics, stories, and gratitude.</summary>
public class ImpactAcceptanceTest : IntegrationTestBase
{
    private record ProjectSummaryResponse(Guid Id, string Name, string Tag, string CurrentPhase);
    private record ImpactMetricResponse(string Value, string? Unit, string Label, bool Highlight);
    private record StoryResponse(int Week, string Text);
    private record GratitudeResponse(string Quote, string Attribution);
    private record ImpactResponse(string Headline, List<ImpactMetricResponse> Metrics, List<StoryResponse> Stories, List<GratitudeResponse> Gratitude);

    [Fact]
    public async Task Impact_screen_returns_the_relationship_headline_and_non_empty_content()
    {
        var client = await CreateAuthenticatedClientAsync();
        var projects = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects");
        var breadAndFish = projects!.Single(p => p.Name == "Bread & Fish");

        var impact = await client.GetFromJsonAsync<ImpactResponse>($"/api/projects/{breadAndFish.Id}/impact");

        Assert.Equal("Impact is friendship, compounded by time.", impact!.Headline);
        Assert.NotEmpty(impact.Metrics);
        Assert.NotEmpty(impact.Stories);
        Assert.NotEmpty(impact.Gratitude);
    }
}
