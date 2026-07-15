using MediatR;

namespace Liturgy.Application.Projects;

public record CreateProjectCommand(string Name, string Tag) : IRequest<ProjectSummaryDto>;
