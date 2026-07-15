// Traces to: L2-WKSP-006, L2-WKSP-007, L2-WKSP-008, L2-WKSP-009, L2-WKSP-010
using System.Net;
using System.Net.Http.Json;
using Liturgy.IntegrationTests.Support;
using Xunit;

namespace Liturgy.IntegrationTests;

/// <summary>
/// Proves the project lifecycle over real HTTP: a project can be renamed, closed (hidden
/// from the default list but revealed with includeClosed), reopened, and permanently deleted.
/// </summary>
public class ProjectLifecycleAcceptanceTest : IntegrationTestBase
{
    private record ProjectSummaryResponse(Guid Id, string Name, string Tag, string CurrentPhase, string Status);

    [Fact]
    public async Task Project_can_be_updated_closed_reopened_and_deleted()
    {
        var client = await CreateAuthenticatedClientAsync();

        var create = await client.PostAsJsonAsync("/api/projects", new { Name = "Lifecycle", Tag = "Original tag" });
        create.EnsureSuccessStatusCode();
        var project = (await create.Content.ReadFromJsonAsync<ProjectSummaryResponse>())!;
        Assert.Equal("Active", project.Status);

        // Update — name and tag change are reflected.
        var update = await client.PutAsJsonAsync($"/api/projects/{project.Id}", new { Name = "Lifecycle Renamed", Tag = "New tag" });
        update.EnsureSuccessStatusCode();
        var updated = (await update.Content.ReadFromJsonAsync<ProjectSummaryResponse>())!;
        Assert.Equal("Lifecycle Renamed", updated.Name);
        Assert.Equal("New tag", updated.Tag);

        // Close — hidden from the default list, visible with includeClosed.
        var close = await client.PostAsync($"/api/projects/{project.Id}/close", null);
        close.EnsureSuccessStatusCode();
        var closed = (await close.Content.ReadFromJsonAsync<ProjectSummaryResponse>())!;
        Assert.Equal("Closed", closed.Status);

        var defaultList = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects");
        Assert.DoesNotContain(defaultList!, p => p.Id == project.Id);

        var withClosed = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects?includeClosed=true");
        Assert.Contains(withClosed!, p => p.Id == project.Id);

        // Reopen — back in the default list.
        var reopen = await client.PostAsync($"/api/projects/{project.Id}/reopen", null);
        reopen.EnsureSuccessStatusCode();
        var reopenedList = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects");
        Assert.Contains(reopenedList!, p => p.Id == project.Id);

        // Delete — gone entirely, even including closed.
        var delete = await client.DeleteAsync($"/api/projects/{project.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var afterDelete = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects?includeClosed=true");
        Assert.DoesNotContain(afterDelete!, p => p.Id == project.Id);

        var getGone = await client.GetAsync($"/api/projects/{project.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getGone.StatusCode);
    }
}
