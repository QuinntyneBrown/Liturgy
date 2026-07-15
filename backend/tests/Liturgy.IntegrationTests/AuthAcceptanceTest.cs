// Traces to: L2-AUTH-001, L2-AUTH-004, L2-AUTH-005, L2-AUTH-009
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Liturgy.IntegrationTests.Support;
using Xunit;

namespace Liturgy.IntegrationTests;

public class AuthAcceptanceTest : IntegrationTestBase
{
    private record CurrentUserResponse(Guid Id, string Email, string FirstName, string LastName, string Role, string Initials);

    [Fact]
    public async Task Register_then_sign_in_then_me_returns_the_authenticated_user()
    {
        var client = Factory.CreateClient();
        var email = $"quinn+{Guid.NewGuid():N}@newhope.dev";

        var register = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { Email = email, FirstName = "Quinn", LastName = "Brown", Password = "Liturgy!2026" });
        register.EnsureSuccessStatusCode();

        var signIn = await client.PostAsJsonAsync(
            "/api/auth/sign-in",
            new { Email = email, Password = "Liturgy!2026" });
        Assert.Equal(HttpStatusCode.OK, signIn.StatusCode);
        var auth = await signIn.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.Equal("QB", auth!.Initials);

        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var meResponse = await client.SendAsync(meRequest);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var me = await meResponse.Content.ReadFromJsonAsync<CurrentUserResponse>();
        Assert.NotNull(me);
        Assert.Equal(email, me!.Email);
        Assert.Equal("Quinn", me.FirstName);
        Assert.Equal("QB", me.Initials);
    }

    [Fact]
    public async Task Me_without_a_bearer_returns_401()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Registering_a_duplicate_email_returns_409()
    {
        var client = Factory.CreateClient();
        var email = $"dupe+{Guid.NewGuid():N}@newhope.dev";
        var body = new { Email = email, FirstName = "Dup", LastName = "Licate", Password = "Liturgy!2026" };

        (await client.PostAsJsonAsync("/api/auth/register", body)).EnsureSuccessStatusCode();
        var second = await client.PostAsJsonAsync("/api/auth/register", body);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }
}
