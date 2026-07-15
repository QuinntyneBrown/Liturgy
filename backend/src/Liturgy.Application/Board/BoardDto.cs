namespace Liturgy.Application.Board;

public record BoardDto(
    Guid ProjectId,
    Guid SprintId,
    int SprintNumber,
    IReadOnlyList<CardDto> Cards);
