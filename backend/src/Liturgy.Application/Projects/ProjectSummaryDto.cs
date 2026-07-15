using Liturgy.Domain;

namespace Liturgy.Application.Projects;

public record ProjectSummaryDto(Guid Id, string Name, string Tag, PhaseKind CurrentPhase, ProjectStatus Status);
