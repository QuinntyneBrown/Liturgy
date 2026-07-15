// Traces to: L2-WKSP-001
using System.Net.Http.Json;
using Liturgy.IntegrationTests.Support;
using Xunit;

namespace Liturgy.IntegrationTests;

/// <summary>Proves the caller's workspace members list resolves named members with initials and role, keyed by user id (for use as a card assignee picker).</summary>
public class MembersAcceptanceTest : IntegrationTestBase
{
    private record MemberResponse(Guid Id, string Name, string Initials, string Role);

    [Fact]
    public async Task Members_endpoint_returns_the_callers_workspace_members()
    {
        var client = await CreateAuthenticatedClientAsync();

        var members = await client.GetFromJsonAsync<List<MemberResponse>>("/api/members");

        Assert.NotEmpty(members!);
        Assert.Contains(members!, m => m.Initials == "QB" && m.Name == "Quinn Brown" && m.Role == "Lead");
    }
}
