using Liturgy.Domain;
using MediatR;

namespace Liturgy.Application.Board;

public record MoveCardCommand(Guid CardId, BoardColumn TargetColumn) : IRequest<CardDto>;
