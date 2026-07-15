using MediatR;

namespace Liturgy.Application.Invitations;

/// <summary>Pending invitations for the caller's workspace.</summary>
public record ListInvitationsQuery : IRequest<IReadOnlyList<InvitationDto>>;
