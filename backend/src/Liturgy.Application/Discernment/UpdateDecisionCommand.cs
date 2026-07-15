using Liturgy.Domain;
using MediatR;

namespace Liturgy.Application.Discernment;

public record UpdateDecisionCommand(Guid ProjectId, DiscernmentPath ChosenPath, string Rationale, string PrayedOverWith) : IRequest<DecisionDto>;
