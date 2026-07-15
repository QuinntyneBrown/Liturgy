using Liturgy.Domain;

namespace Liturgy.Application.Loop;

public record CardLoopDto(
    Guid CardId,
    Guid ProjectId,
    string Code,
    string Title,
    BoardColumn Column,
    RKind? CurrentR,
    int LoggedCount,
    bool CanMarkDone,
    IReadOnlyList<MovementDto> Movements);
