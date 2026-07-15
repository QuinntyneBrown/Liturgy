using MediatR;

namespace Liturgy.Application.Invitations;

/// <summary>A Lead cancels a pending invitation.</summary>
public record RevokeInvitationCommand(Guid Id) : IRequest;
