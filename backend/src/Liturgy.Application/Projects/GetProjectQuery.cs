using MediatR;

namespace Liturgy.Application.Projects;

public record GetProjectQuery(Guid ProjectId) : IRequest<ProjectJourneyDto>;
