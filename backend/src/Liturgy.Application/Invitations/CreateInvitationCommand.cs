using MediatR;

namespace Liturgy.Application.Invitations;

public record CreateInvitationCommand(string Email, string? Role) : IRequest<InvitationDto>;
