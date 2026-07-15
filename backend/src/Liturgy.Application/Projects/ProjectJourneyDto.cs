using Liturgy.Domain;

namespace Liturgy.Application.Projects;

public record ProjectJourneyDto(
    Guid Id,
    string Name,
    string Tag,
    PhaseKind CurrentPhase,
    IReadOnlyList<PhaseDto> Phases);
