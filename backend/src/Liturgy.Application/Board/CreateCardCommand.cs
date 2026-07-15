using MediatR;

namespace Liturgy.Application.Board;

public record CreateCardCommand(Guid ProjectId, string Title, string? Description, Guid? AssigneeId) : IRequest<CardDto>;
