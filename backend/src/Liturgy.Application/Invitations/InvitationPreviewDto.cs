namespace Liturgy.Application.Invitations;

/// <summary>
/// The public, anonymous view of a pending invitation, shown on the sign-up screen so an
/// invitee sees which account they've been asked to join.
/// </summary>
public record InvitationPreviewDto(string WorkspaceName, string InvitedByName, string Email);
