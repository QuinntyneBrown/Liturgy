using MediatR;

namespace Liturgy.Application.Discernment;

public record GetDecisionQuery(Guid ProjectId) : IRequest<DecisionDto>;
