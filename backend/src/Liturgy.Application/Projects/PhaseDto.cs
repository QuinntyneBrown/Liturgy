using Liturgy.Domain;

namespace Liturgy.Application.Projects;

public record PhaseDto(
    Guid Id,
    PhaseKind Kind,
    PhaseState State,
    int Order,
    GateDto? Gate);
