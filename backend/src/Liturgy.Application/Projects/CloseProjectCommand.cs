using MediatR;

namespace Liturgy.Application.Projects;

/// <summary>Soft-hides a project (sets <see cref="Domain.ProjectStatus.Closed"/>) without deleting it.</summary>
public record CloseProjectCommand(Guid Id) : IRequest<ProjectSummaryDto>;
