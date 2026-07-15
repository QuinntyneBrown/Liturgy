using MediatR;

namespace Liturgy.Application.Projects;

public record ListProjectsQuery : IRequest<IReadOnlyList<ProjectSummaryDto>>;
