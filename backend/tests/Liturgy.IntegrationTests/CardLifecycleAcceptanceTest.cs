// Traces to: L2-WORK-014, L2-WORK-015, L2-WORK-016, L2-WORK-017, L2-WORK-018, L2-WORK-019, L2-WORK-020
using System.Net;
using System.Net.Http.Json;
using Liturgy.IntegrationTests.Support;
using Xunit;

namespace Liturgy.IntegrationTests;

/// <summary>
/// Proves the card lifecycle over real HTTP: a card carries a description and story points,
/// and cancel/close lift it off the active board (independent of the 5R Done gate) while
/// reopen restores it and delete removes it entirely.
/// </summary>
public class CardLifecycleAcceptanceTest : IntegrationTestBase
{
    private record ProjectSummaryResponse(Guid Id, string Name, string Tag, string CurrentPhase, string Status);
    private record CardResponse(
        Guid Id, Guid ProjectId, string Code, string Title, string? Description, int? Points,
        Guid? AssigneeId, string Column, string Status, int LoggedCount);
    private record BoardResponse(Guid ProjectId, Guid SprintId, int SprintNumber, List<CardResponse> Cards);

    private async Task<Guid> LanternIdAsync(HttpClient client)
    {
        var projects = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects");
        return projects!.Single(p => p.Name == "Lantern").Id;
    }

    private async Task<List<CardResponse>> BoardCardsAsync(HttpClient client, Guid projectId)
    {
        var board = await client.GetFromJsonAsync<BoardResponse>($"/api/board/{projectId}");
        return board!.Cards;
    }

    [Fact]
    public async Task Card_carries_description_and_points_and_status_transitions_move_it_off_and_back_onto_the_board()
    {
        var client = await CreateAuthenticatedClientAsync();
        var projectId = await LanternIdAsync(client);

        // Create with a description.
        var create = await client.PostAsJsonAsync("/api/board/cards", new
        {
            ProjectId = projectId,
            Title = "Escalation runbook",
            Description = "Step-by-step runbook for escalating a call.",
            AssigneeId = (Guid?)null
        });
        create.EnsureSuccessStatusCode();
        var card = (await create.Content.ReadFromJsonAsync<CardResponse>())!;
        Assert.Equal("Step-by-step runbook for escalating a call.", card.Description);
        Assert.Equal("Open", card.Status);
        Assert.Null(card.Points);

        // Point it.
        var point = await client.PostAsJsonAsync($"/api/board/cards/{card.Id}/point", new { Points = 5 });
        point.EnsureSuccessStatusCode();
        var pointed = (await point.Content.ReadFromJsonAsync<CardResponse>())!;
        Assert.Equal(5, pointed.Points);

        Assert.Contains(await BoardCardsAsync(client, projectId), c => c.Id == card.Id);

        // Cancel — leaves the active board even though its 5R loop is untouched.
        var cancel = await client.PostAsync($"/api/board/cards/{card.Id}/cancel", null);
        cancel.EnsureSuccessStatusCode();
        var cancelled = (await cancel.Content.ReadFromJsonAsync<CardResponse>())!;
        Assert.Equal("Cancelled", cancelled.Status);
        Assert.DoesNotContain(await BoardCardsAsync(client, projectId), c => c.Id == card.Id);

        // Reopen — back on the board.
        var reopen = await client.PostAsync($"/api/board/cards/{card.Id}/reopen", null);
        reopen.EnsureSuccessStatusCode();
        Assert.Contains(await BoardCardsAsync(client, projectId), c => c.Id == card.Id);

        // Close — also leaves the board.
        var close = await client.PostAsync($"/api/board/cards/{card.Id}/close", null);
        close.EnsureSuccessStatusCode();
        var closed = (await close.Content.ReadFromJsonAsync<CardResponse>())!;
        Assert.Equal("Closed", closed.Status);
        Assert.DoesNotContain(await BoardCardsAsync(client, projectId), c => c.Id == card.Id);

        // Delete — gone entirely (reopen first so it's a plain delete of a live card).
        await client.PostAsync($"/api/board/cards/{card.Id}/reopen", null);
        var delete = await client.DeleteAsync($"/api/board/cards/{card.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.DoesNotContain(await BoardCardsAsync(client, projectId), c => c.Id == card.Id);
    }
}
