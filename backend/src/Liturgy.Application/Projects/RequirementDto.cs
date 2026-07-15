using Liturgy.Domain;

namespace Liturgy.Application.Projects;

public record RequirementDto(Guid Id, string Label, string? Meta, RequirementState State, int Order)
{
    public static RequirementDto From(Requirement r) => new(r.Id, r.Label, r.Meta, r.State, r.Order);
}
