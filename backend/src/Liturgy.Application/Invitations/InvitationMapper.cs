using Liturgy.Domain;

namespace Liturgy.Application.Invitations;

public static class InvitationMapper
{
    public static InvitationDto ToDto(Invitation invitation) => new(
        invitation.Id,
        invitation.Email,
        invitation.Role,
        invitation.Status,
        invitation.Token,
        InvitationToken.PathFor(invitation.Token),
        invitation.CreatedAt);
}
