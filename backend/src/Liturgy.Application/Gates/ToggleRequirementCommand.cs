using Liturgy.Application.Projects;
using MediatR;

namespace Liturgy.Application.Gates;

public record ToggleRequirementCommand(Guid RequirementId, bool Done) : IRequest<GateDto>;
