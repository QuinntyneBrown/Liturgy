using Liturgy.Domain;

namespace Liturgy.Application.Board;

public record CardDto(
    Guid Id,
    Guid ProjectId,
    Guid SprintId,
    string Code,
    string Title,
    Guid? AssigneeId,
    string? AssigneeInitials,
    BoardColumn Column,
    RKind? CurrentR,
    bool IsBlocked,
    int LoggedCount);
