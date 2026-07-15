using MediatR;

namespace Liturgy.Application.Members;

public record GetMembersQuery : IRequest<IReadOnlyList<MemberDto>>;
