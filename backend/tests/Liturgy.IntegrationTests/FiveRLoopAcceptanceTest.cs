// Traces to: L2-WORK-004, L2-WORK-008, L2-WORK-009, L2-WORK-010, L2-WORK-012
using System.Net;
using System.Net.Http.Json;
using Liturgy.IntegrationTests.Support;
using Xunit;

namespace Liturgy.IntegrationTests;

/// <summary>
/// Proves the 5R enforcement over real HTTP: a card cannot reach Done until every R
/// movement is logged, movements must be logged in loop order, and once complete the
/// card moves to Done.
/// </summary>
public class FiveRLoopAcceptanceTest : IntegrationTestBase
{
    private record ProjectSummaryResponse(Guid Id, string Name, string Tag, string CurrentPhase);
    private record CardResponse(Guid Id, Guid ProjectId, Guid SprintId, string Code, string Title, Guid? AssigneeId, string? AssigneeInitials, string Column, string? CurrentR, bool IsBlocked, int LoggedCount);
    private record MovementResponse(Guid Id, string Kind, int Order, string State, string? Ask, string? Received, string? Synthesis, string? ArtifactUrl, string? WhatChanged, string? Thanksgiving);
    private record LoopResponse(Guid CardId, string Code, string Title, string Column, string? CurrentR, int LoggedCount, bool CanMarkDone, List<MovementResponse> Movements);

    private static readonly string[] LoopOrder = { "Request", "Receive", "Review", "Render", "Rejoice" };

    private static async Task<CardResponse> CreateCardAsync(HttpClient client, Guid projectId, string title)
    {
        var create = await client.PostAsJsonAsync("/api/board/cards", new { ProjectId = projectId, Title = title, AssigneeId = (Guid?)null });
        var body = await create.Content.ReadAsStringAsync();
        Assert.True(create.IsSuccessStatusCode, $"create card status {(int)create.StatusCode}: {body}");
        return (await create.Content.ReadFromJsonAsync<CardResponse>())!;
    }

    [Fact]
    public async Task Card_cannot_reach_Done_until_the_full_5R_loop_is_logged()
    {
        var client = await CreateAuthenticatedClientAsync();
        var projects = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects");
        var lantern = projects!.Single(p => p.Name == "Lantern");

        var card = await CreateCardAsync(client, lantern.Id, "Test work item");
        Assert.Equal("Request", card.CurrentR);
        Assert.Equal(0, card.LoggedCount);

        // Moving to Done with an empty loop is rejected.
        var earlyMove = await client.PostAsJsonAsync($"/api/board/cards/{card.Id}/move", new { Column = "Done" });
        Assert.Equal(HttpStatusCode.Conflict, earlyMove.StatusCode);

        // Logging out of order is rejected.
        var outOfOrder = await client.PostAsJsonAsync(
            $"/api/loop/cards/{card.Id}/movements",
            new { Kind = "Render", ArtifactUrl = "prs/test" });
        Assert.Equal(HttpStatusCode.BadRequest, outOfOrder.StatusCode);

        // Log all five in order.
        LoopResponse? loop = null;
        foreach (var kind in LoopOrder)
        {
            var log = await client.PostAsJsonAsync(
                $"/api/loop/cards/{card.Id}/movements",
                new { Kind = kind, Ask = "ask", Received = "received", Synthesis = "synthesis", ArtifactUrl = "prs/test", WhatChanged = "changed", Thanksgiving = "thanks" });
            log.EnsureSuccessStatusCode();
            loop = await log.Content.ReadFromJsonAsync<LoopResponse>();
        }

        Assert.NotNull(loop);
        Assert.Equal(5, loop!.LoggedCount);
        Assert.True(loop.CanMarkDone);
        Assert.Null(loop.CurrentR);

        // Now the move to Done succeeds.
        var move = await client.PostAsJsonAsync($"/api/board/cards/{card.Id}/move", new { Column = "Done" });
        move.EnsureSuccessStatusCode();
        var moved = await move.Content.ReadFromJsonAsync<CardResponse>();
        Assert.Equal("Done", moved!.Column);
    }

    [Fact]
    public async Task Logging_a_movement_advances_the_current_R()
    {
        var client = await CreateAuthenticatedClientAsync();
        var projects = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects");
        var lantern = projects!.Single(p => p.Name == "Lantern");

        var card = await CreateCardAsync(client, lantern.Id, "Advance test");

        var log = await client.PostAsJsonAsync(
            $"/api/loop/cards/{card.Id}/movements",
            new { Kind = "Request", Ask = "invite the Spirit" });
        log.EnsureSuccessStatusCode();
        var loop = await log.Content.ReadFromJsonAsync<LoopResponse>();

        Assert.Equal("Receive", loop!.CurrentR);
        Assert.Equal(1, loop.LoggedCount);
        Assert.Equal("InLoop", loop.Column);
        var request = loop.Movements.Single(m => m.Kind == "Request");
        Assert.Equal("Done", request.State);
        var receive = loop.Movements.Single(m => m.Kind == "Receive");
        Assert.Equal("Current", receive.State);
    }
}
