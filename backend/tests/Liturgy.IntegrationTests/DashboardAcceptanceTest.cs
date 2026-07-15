// Traces to: L2-WKSP-005
using System.Net.Http.Json;
using Liturgy.IntegrationTests.Support;
using Xunit;

namespace Liturgy.IntegrationTests;

/// <summary>Proves the workspace dashboard aggregates momentum, attention items, and 4D lanes across the caller's workspaces.</summary>
public class DashboardAcceptanceTest : IntegrationTestBase
{
    private record MomentumResponse(int ActiveProjects, int MovementsThisWeek, int GatesBlocked, int WeeksCompounded);
    private record AttentionResponse(Guid ProjectId, string ProjectName, string Title, string Reason, string ActionKind, Guid ActionTargetId);
    private record ProjectLaneResponse(Guid Id, string Name, string CurrentPhase, string Meta, bool Blocked);
    private record DashboardResponse(MomentumResponse Momentum, List<AttentionResponse> Attention, List<ProjectLaneResponse> Projects);

    [Fact]
    public async Task Dashboard_aggregates_momentum_attention_and_lanes_across_the_workspace()
    {
        var client = await CreateAuthenticatedClientAsync();

        var dashboard = await client.GetFromJsonAsync<DashboardResponse>("/api/dashboard");

        Assert.True(dashboard!.Momentum.ActiveProjects > 0);
        Assert.True(dashboard.Momentum.GatesBlocked > 0);
        Assert.NotEmpty(dashboard.Projects);
        Assert.NotEmpty(dashboard.Attention);
    }
}
