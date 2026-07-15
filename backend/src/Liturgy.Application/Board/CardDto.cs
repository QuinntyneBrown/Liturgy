using Liturgy.Domain;

namespace Liturgy.Application.Board;

public record CardDto(
    Guid Id,
    Guid ProjectId,
    Guid SprintId,
    string Code,
    string Title,
    string? Description,
    int? Points,
    Guid? AssigneeId,
    string? AssigneeInitials,
    BoardColumn Column,
    CardStatus Status,
    RKind? CurrentR,
    bool IsBlocked,
    int LoggedCount);
