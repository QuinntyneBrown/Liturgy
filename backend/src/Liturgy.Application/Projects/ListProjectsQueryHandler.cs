using Liturgy.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Projects;

public class ListProjectsQueryHandler : IRequestHandler<ListProjectsQuery, IReadOnlyList<ProjectSummaryDto>>
{
    private readonly IAppDbContext _db;
    private readonly IWorkspaceAccess _workspaceAccess;

    public ListProjectsQueryHandler(IAppDbContext db, IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<IReadOnlyList<ProjectSummaryDto>> Handle(ListProjectsQuery request, CancellationToken cancellationToken)
    {
        var workspaceIds = await _workspaceAccess.MyWorkspaceIdsAsync(cancellationToken);

        return await _db.Projects
            .AsNoTracking()
            .Where(p => workspaceIds.Contains(p.WorkspaceId))
            .OrderBy(p => p.CreatedAt)
            .Select(p => new ProjectSummaryDto(p.Id, p.Name, p.Tag, p.CurrentPhase))
            .ToListAsync(cancellationToken);
    }
}
