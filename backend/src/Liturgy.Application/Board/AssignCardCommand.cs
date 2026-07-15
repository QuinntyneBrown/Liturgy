using MediatR;

namespace Liturgy.Application.Board;

public record AssignCardCommand(Guid CardId, Guid? AssigneeId) : IRequest<CardDto>;
