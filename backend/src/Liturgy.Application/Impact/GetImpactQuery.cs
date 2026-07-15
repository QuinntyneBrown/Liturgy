using MediatR;

namespace Liturgy.Application.Impact;

public record GetImpactQuery(Guid ProjectId) : IRequest<ImpactDto>;
