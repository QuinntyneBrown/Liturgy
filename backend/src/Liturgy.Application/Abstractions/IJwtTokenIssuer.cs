using Liturgy.Domain;

namespace Liturgy.Application.Abstractions;

public interface IJwtTokenIssuer
{
    string Issue(User user);
}
