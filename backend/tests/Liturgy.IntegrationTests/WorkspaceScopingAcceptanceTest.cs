// Traces to: L2-WKSP-003, L2-SEC-001
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Liturgy.IntegrationTests.Support;
using Xunit;

namespace Liturgy.IntegrationTests;

/// <summary>
/// Proves workspace scoping: a project is visible only to members of its workspace. A
/// freshly-registered user is seeded their own empty personal workspace and must not
/// see (or even learn of the existence of) another workspace's projects.
/// </summary>
public class WorkspaceScopingAcceptanceTest : IntegrationTestBase
{
    private record ProjectSummaryResponse(Guid Id, string Name, string Tag, string CurrentPhase);

    private async Task<HttpClient> RegisterFreshUserAsync()
    {
        var client = Factory.CreateClient();
        var email = $"stranger+{Guid.NewGuid():N}@example.com";

        var register = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { Email = email, FirstName = "Stranger", LastName = "Danger", Password = "Liturgy!2026" });
        register.EnsureSuccessStatusCode();

        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        return client;
    }

    [Fact]
    public async Task A_freshly_registered_user_cannot_see_another_workspaces_project()
    {
        var member = await CreateAuthenticatedClientAsync();
        var memberProjects = await member.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects");
        var lantern = memberProjects!.Single(p => p.Name == "Lantern");

        var stranger = await RegisterFreshUserAsync();

        var get = await stranger.GetAsync($"/api/projects/{lantern.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        var strangerProjects = await stranger.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects");
        Assert.Empty(strangerProjects!);
    }
}
