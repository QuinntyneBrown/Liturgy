using MediatR;

namespace Liturgy.Application.Invitations;

/// <summary>Anonymous preview of a pending invitation, keyed by token.</summary>
public record GetInvitationByTokenQuery(string Token) : IRequest<InvitationPreviewDto>;
