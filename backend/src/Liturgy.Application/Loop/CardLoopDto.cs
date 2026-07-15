using Liturgy.Domain;

namespace Liturgy.Application.Loop;

public record CardLoopDto(
    Guid CardId,
    string Code,
    string Title,
    BoardColumn Column,
    RKind? CurrentR,
    int LoggedCount,
    bool CanMarkDone,
    IReadOnlyList<MovementDto> Movements);
