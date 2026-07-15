using MediatR;

namespace Liturgy.Application.Projects;

/// <summary>Permanently deletes a project and all of its dependent data.</summary>
public record DeleteProjectCommand(Guid Id) : IRequest;
