using MediatR;

namespace Liturgy.Application.Auth;

public record RegisterCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string? InvitationToken = null) : IRequest<AuthResult>;
