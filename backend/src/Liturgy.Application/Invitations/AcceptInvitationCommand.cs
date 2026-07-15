using MediatR;

namespace Liturgy.Application.Invitations;

/// <summary>An authenticated user accepts a pending invitation and joins the inviting workspace.</summary>
public record AcceptInvitationCommand(string Token) : IRequest;
