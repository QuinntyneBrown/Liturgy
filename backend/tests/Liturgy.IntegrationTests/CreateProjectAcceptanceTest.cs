// Traces to: L2-WKSP-003
using System.Net.Http.Json;
using Liturgy.IntegrationTests.Support;
using Xunit;

namespace Liturgy.IntegrationTests;

/// <summary>Proves creating a project scaffolds the standard 4D journey (Discover current, a Blocked gate) and that it appears in the caller's project list.</summary>
public class CreateProjectAcceptanceTest : IntegrationTestBase
{
    private record ProjectSummaryResponse(Guid Id, string Name, string Tag, string CurrentPhase);
    private record RequirementResponse(Guid Id, string Label, string? Meta, string State, int Order);
    private record GateResponse(Guid Id, Guid PhaseId, string Title, string State, List<RequirementResponse> Requirements);
    private record PhaseResponse(Guid Id, string Kind, string State, int Order, GateResponse? Gate);
    private record JourneyResponse(Guid Id, string Name, string Tag, string CurrentPhase, List<PhaseResponse> Phases);

    [Fact]
    public async Task Creating_a_project_scaffolds_the_4D_journey_with_Discover_current()
    {
        var client = await CreateAuthenticatedClientAsync();

        var create = await client.PostAsJsonAsync("/api/projects", new { Name = "New Well", Tag = "Test project" });
        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<ProjectSummaryResponse>();
        Assert.Equal("Discover", created!.CurrentPhase);

        var journey = await client.GetFromJsonAsync<JourneyResponse>($"/api/projects/{created.Id}");
        var discover = journey!.Phases.Single(p => p.Kind == "Discover");
        Assert.Equal("Current", discover.State);
        Assert.NotNull(discover.Gate);
        Assert.Equal("Blocked", discover.Gate!.State);

        var projects = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects");
        Assert.Contains(projects!, p => p.Id == created.Id);
    }
}
