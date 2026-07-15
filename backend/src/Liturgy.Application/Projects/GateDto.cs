using Liturgy.Domain;

namespace Liturgy.Application.Projects;

public record GateDto(
    Guid Id,
    Guid PhaseId,
    string Title,
    GateState State,
    IReadOnlyList<RequirementDto> Requirements)
{
    public static GateDto From(Gate gate, IEnumerable<Requirement> requirements) => new(
        gate.Id,
        gate.PhaseId,
        gate.Title,
        gate.State,
        requirements.OrderBy(r => r.Order).Select(RequirementDto.From).ToList());
}
