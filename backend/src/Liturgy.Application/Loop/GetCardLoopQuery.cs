using MediatR;

namespace Liturgy.Application.Loop;

public record GetCardLoopQuery(Guid CardId) : IRequest<CardLoopDto>;
