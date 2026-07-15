using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Projects;

public class CloseProjectCommandHandler : IRequestHandler<CloseProjectCommand, ProjectSummaryDto>
{
    private readonly IAppDbContext _db;
    private readonly IWorkspaceAccess _workspaceAccess;

    public CloseProjectCommandHandler(IAppDbContext db, IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<ProjectSummaryDto> Handle(CloseProjectCommand request, CancellationToken cancellationToken)
    {
        await _workspaceAccess.EnsureProjectVisibleAsync(request.Id, cancellationToken);

        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new ProjectNotFoundException(request.Id);

        project.Status = ProjectStatus.Closed;
        await _db.SaveChangesAsync(cancellationToken);

        return new ProjectSummaryDto(project.Id, project.Name, project.Tag, project.CurrentPhase, project.Status);
    }
}
