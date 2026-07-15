using MediatR;

namespace Liturgy.Application.Auth;

public record SignInCommand(string Email, string Password) : IRequest<AuthResult>;
