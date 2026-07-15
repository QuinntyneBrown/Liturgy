namespace Liturgy.Domain;

/// <summary>
/// Lifecycle status of an <see cref="Invitation"/> to join a workspace (account).
/// </summary>
public enum InvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Revoked = 2
}
