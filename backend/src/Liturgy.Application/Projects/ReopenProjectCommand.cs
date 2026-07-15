using MediatR;

namespace Liturgy.Application.Projects;

/// <summary>Restores a closed project to <see cref="Domain.ProjectStatus.Active"/>.</summary>
public record ReopenProjectCommand(Guid Id) : IRequest<ProjectSummaryDto>;
