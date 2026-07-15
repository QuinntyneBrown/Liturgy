using System.Security.Cryptography;

namespace Liturgy.Application.Invitations;

/// <summary>Generates opaque, URL-safe invitation tokens.</summary>
public static class InvitationToken
{
    public static string New()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public static string PathFor(string token) => $"/sign-up?token={token}";
}
