using MediatR;

namespace Liturgy.Application.Auth;

public record RegisterCommand(string Email, string FirstName, string LastName, string Password) : IRequest<AuthResult>;
