using MediatR;

namespace Liturgy.Application.Auth;

public record GetCurrentUserQuery : IRequest<CurrentUserDto>;
