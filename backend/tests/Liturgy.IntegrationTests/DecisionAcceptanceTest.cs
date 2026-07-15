// Traces to: L2-PLAY-011
using System.Net.Http.Json;
using Liturgy.IntegrationTests.Support;
using Xunit;

namespace Liturgy.IntegrationTests;

/// <summary>
/// Proves recording a discernment decision satisfies the Discern gate's "path chosen"
/// requirement by reusing the gate toggle mechanic, while an unrelated requirement
/// ("Sprint goal defined") keeps the gate Blocked.
/// </summary>
public class DecisionAcceptanceTest : IntegrationTestBase
{
    private record ProjectSummaryResponse(Guid Id, string Name, string Tag, string CurrentPhase);
    private record DecisionResponse(string? ChosenPath, string Rationale, string PrayedOverWith, string? DecidedAt);
    private record RequirementResponse(Guid Id, string Label, string? Meta, string State, int Order);
    private record GateResponse(Guid Id, Guid PhaseId, string Title, string State, List<RequirementResponse> Requirements);
    private record PhaseResponse(Guid Id, string Kind, string State, int Order, GateResponse? Gate);
    private record JourneyResponse(Guid Id, string Name, string Tag, string CurrentPhase, List<PhaseResponse> Phases);

    [Fact]
    public async Task Recording_a_decision_satisfies_the_path_chosen_requirement_but_leaves_the_gate_blocked()
    {
        var client = await CreateAuthenticatedClientAsync();
        var projects = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects");
        var wellspring = projects!.Single(p => p.Name == "Wellspring");

        var put = await client.PutAsJsonAsync(
            $"/api/projects/{wellspring.Id}/decision",
            new { ChosenPath = "Reimagine", Rationale = "Reimagining around the women who walk for water.", PrayedOverWith = "Whole team" });
        put.EnsureSuccessStatusCode();
        var decision = await put.Content.ReadFromJsonAsync<DecisionResponse>();
        Assert.Equal("Reimagine", decision!.ChosenPath);

        var journey = await client.GetFromJsonAsync<JourneyResponse>($"/api/projects/{wellspring.Id}");
        var discern = journey!.Phases.Single(p => p.Kind == "Discern");
        var gate = discern.Gate!;

        var pathRequirement = gate.Requirements.Single(r => r.Label.Contains("path", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("Done", pathRequirement.State);

        var sprintGoal = gate.Requirements.Single(r => r.Label.Contains("Sprint goal", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("Todo", sprintGoal.State);
        Assert.Equal("Blocked", gate.State);
    }
}
