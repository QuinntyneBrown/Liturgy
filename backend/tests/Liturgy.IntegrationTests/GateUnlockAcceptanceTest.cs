// Traces to: L2-PLAY-004, L2-PLAY-005, L2-PLAY-006
using System.Net.Http.Json;
using Liturgy.IntegrationTests.Support;
using Xunit;

namespace Liturgy.IntegrationTests;

/// <summary>
/// Proves the 4D gate mechanic over real HTTP: completing the last requirement on a
/// blocked gate opens it and unlocks the next phase.
/// </summary>
public class GateUnlockAcceptanceTest : IntegrationTestBase
{
    private record ProjectSummaryResponse(Guid Id, string Name, string Tag, string CurrentPhase);
    private record RequirementResponse(Guid Id, string Label, string? Meta, string State, int Order);
    private record GateResponse(Guid Id, Guid PhaseId, string Title, string State, List<RequirementResponse> Requirements);
    private record PhaseResponse(Guid Id, string Kind, string State, int Order, GateResponse? Gate);
    private record JourneyResponse(Guid Id, string Name, string Tag, string CurrentPhase, List<PhaseResponse> Phases);

    [Fact]
    public async Task Completing_the_last_requirement_opens_the_gate_and_unlocks_the_next_phase()
    {
        var client = await CreateAuthenticatedClientAsync();

        var projects = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects");
        var lantern = projects!.Single(p => p.Name == "Lantern");

        var journey = await client.GetFromJsonAsync<JourneyResponse>($"/api/projects/{lantern.Id}");
        var blockedPhase = journey!.Phases.Single(p => p.Gate is { State: "Blocked" });
        var gate = blockedPhase.Gate!;
        var todo = gate.Requirements.Single(r => r.State == "Todo");

        var nextPhase = journey.Phases.Single(p => p.Order == blockedPhase.Order + 1);
        Assert.Equal("Locked", nextPhase.State);

        var toggle = await client.PostAsJsonAsync(
            $"/api/gates/requirements/{todo.Id}/toggle",
            new { Done = true });
        toggle.EnsureSuccessStatusCode();
        var updatedGate = await toggle.Content.ReadFromJsonAsync<GateResponse>();
        Assert.Equal("Open", updatedGate!.State);

        var after = await client.GetFromJsonAsync<JourneyResponse>($"/api/projects/{lantern.Id}");
        var afterBlocked = after!.Phases.Single(p => p.Id == blockedPhase.Id);
        var afterNext = after.Phases.Single(p => p.Id == nextPhase.Id);

        Assert.Equal("Done", afterBlocked.State);
        Assert.Equal("Current", afterNext.State);
        Assert.Equal(afterNext.Kind, after.CurrentPhase);
    }
}
