using MediatR;

namespace Liturgy.Application.Board;

public record CreateCardCommand(Guid ProjectId, string Title, Guid? AssigneeId) : IRequest<CardDto>;
