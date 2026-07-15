// Traces to: L2-WKSP-011, L2-WKSP-012, L2-WKSP-013, L2-WKSP-016, L2-AUTH-016
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Liturgy.IntegrationTests.Support;
using Xunit;

namespace Liturgy.IntegrationTests;

/// <summary>
/// Proves the invitation flow over real HTTP: a Lead invites an email, the invite is
/// listable and previewable anonymously, a non-Lead cannot invite, and registering with the
/// token drops the new user into the inviting account (New Hope Collective) rather than a
/// fresh personal workspace.
/// </summary>
public class InvitationAcceptanceTest : IntegrationTestBase
{
    private record InvitationResponse(Guid Id, string Email, string Role, string Status, string Token, string InvitePath, DateTimeOffset CreatedAt);
    private record PreviewResponse(string WorkspaceName, string InvitedByName, string Email);
    private record ProjectSummaryResponse(Guid Id, string Name, string Tag, string CurrentPhase, string Status);

    [Fact]
    public async Task Lead_invites_an_email_and_registering_with_the_token_joins_the_inviting_account()
    {
        var lead = await CreateAuthenticatedClientAsync(); // quinn@newhope.dev is a Lead

        var email = $"invitee-{Guid.NewGuid():N}@example.com";
        var create = await lead.PostAsJsonAsync("/api/invitations", new { Email = email, Role = "Developer" });
        create.EnsureSuccessStatusCode();
        var invite = (await create.Content.ReadFromJsonAsync<InvitationResponse>())!;
        Assert.Equal("Pending", invite.Status);
        Assert.Equal($"/sign-up?token={invite.Token}", invite.InvitePath);

        // Listable while pending.
        var pending = await lead.GetFromJsonAsync<List<InvitationResponse>>("/api/invitations");
        Assert.Contains(pending!, i => i.Id == invite.Id);

        // Anonymous preview shows which account the invite is for.
        var anon = Factory.CreateClient();
        var preview = await anon.GetFromJsonAsync<PreviewResponse>($"/api/invitations/{invite.Token}");
        Assert.Equal("New Hope Collective", preview!.WorkspaceName);
        Assert.Equal(email, preview.Email);

        // Register with the token → land in New Hope Collective (sees Lantern), no personal workspace.
        var register = await anon.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            FirstName = "New",
            LastName = "Joiner",
            Password = "Welcome!2026a",
            InvitationToken = invite.Token
        });
        register.EnsureSuccessStatusCode();
        var auth = (await register.Content.ReadFromJsonAsync<AuthResponse>())!;

        var joined = Factory.CreateClient();
        joined.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var projects = await joined.GetFromJsonAsync<List<ProjectSummaryResponse>>("/api/projects");
        Assert.Contains(projects!, p => p.Name == "Lantern");

        // The invite is no longer pending.
        var afterAccept = await lead.GetFromJsonAsync<List<InvitationResponse>>("/api/invitations");
        Assert.DoesNotContain(afterAccept!, i => i.Id == invite.Id);
    }

    [Fact]
    public async Task A_non_Lead_cannot_create_an_invitation()
    {
        var client = Factory.CreateClient();
        var signIn = await client.PostAsJsonAsync("/api/auth/sign-in", new { Email = "jonah@newhope.dev", Password = "Liturgy!2026" });
        signIn.EnsureSuccessStatusCode();
        var auth = (await signIn.Content.ReadFromJsonAsync<AuthResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var create = await client.PostAsJsonAsync("/api/invitations", new { Email = "someone@example.com", Role = (string?)null });
        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
    }
}
