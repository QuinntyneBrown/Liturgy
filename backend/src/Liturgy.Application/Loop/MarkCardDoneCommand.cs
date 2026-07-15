using Liturgy.Application.Board;
using MediatR;

namespace Liturgy.Application.Loop;

public record MarkCardDoneCommand(Guid CardId) : IRequest<CardDto>;
