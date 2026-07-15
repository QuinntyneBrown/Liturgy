using MediatR;

namespace Liturgy.Application.Board;

public record GetBoardQuery(Guid ProjectId) : IRequest<BoardDto>;
