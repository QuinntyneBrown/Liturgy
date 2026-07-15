using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Workspaces;

public class WorkspaceAccess : IWorkspaceAccess
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public WorkspaceAccess(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task EnsureProjectVisibleAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var workspaceId = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId)
            .Select(p => (Guid?)p.WorkspaceId)
            .FirstOrDefaultAsync(cancellationToken);

        if (workspaceId is null)
        {
            throw new ProjectNotFoundException(projectId);
        }

        var myWorkspaceIds = await MyWorkspaceIdsAsync(cancellationToken);
        if (!myWorkspaceIds.Contains(workspaceId.Value))
        {
            throw new ProjectNotFoundException(projectId);
        }
    }

    public async Task<IReadOnlyList<Guid>> MyWorkspaceIdsAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Array.Empty<Guid>();
        }

        return await _db.Memberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => m.WorkspaceId)
            .ToListAsync(cancellationToken);
    }
}
