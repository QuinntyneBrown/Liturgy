namespace Liturgy.Domain;

/// <summary>
/// A pending invitation for someone to join a <see cref="Workspace"/> (account). Created by
/// a Lead, carried by an opaque <see cref="Token"/>. When the invited email registers — or
/// an existing user accepts — a <see cref="Membership"/> is created and the invite is marked
/// <see cref="InvitationStatus.Accepted"/>.
/// </summary>
public class Invitation
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public Guid InvitedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
}
