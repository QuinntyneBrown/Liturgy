using MediatR;

namespace Liturgy.Application.Projects;

public record UpdateProjectCommand(Guid Id, string Name, string Tag) : IRequest<ProjectSummaryDto>;
