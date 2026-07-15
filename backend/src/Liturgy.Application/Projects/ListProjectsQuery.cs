using MediatR;

namespace Liturgy.Application.Projects;

/// <summary>Lists the caller's projects. Closed projects are hidden unless <paramref name="IncludeClosed"/> is set.</summary>
public record ListProjectsQuery(bool IncludeClosed = false) : IRequest<IReadOnlyList<ProjectSummaryDto>>;
