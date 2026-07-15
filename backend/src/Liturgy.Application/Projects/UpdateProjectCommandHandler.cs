using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Projects;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectSummaryDto>
{
    private readonly IAppDbContext _db;
    private readonly IWorkspaceAccess _workspaceAccess;

    public UpdateProjectCommandHandler(IAppDbContext db, IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<ProjectSummaryDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        await _workspaceAccess.EnsureProjectVisibleAsync(request.Id, cancellationToken);

        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new ProjectNotFoundException(request.Id);

        project.Name = request.Name.Trim();
        project.Tag = request.Tag.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        return new ProjectSummaryDto(project.Id, project.Name, project.Tag, project.CurrentPhase, project.Status);
    }
}
