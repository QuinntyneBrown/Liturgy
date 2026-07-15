namespace Liturgy.Application.Abstractions;

/// <summary>
/// Enforces workspace membership as the project visibility boundary. Every
/// project-scoped handler must call <see cref="EnsureProjectVisibleAsync"/> before it
/// mutates or returns anything, so a caller can never see or affect a project outside
/// their own workspaces.
/// </summary>
public interface IWorkspaceAccess
{
    /// <summary>Throws <see cref="Enforcement.ProjectNotFoundException"/> if the project doesn't exist, or the current user isn't a member of its workspace. The two cases are indistinguishable to the caller by design.</summary>
    Task EnsureProjectVisibleAsync(Guid projectId, CancellationToken cancellationToken);

    /// <summary>Workspace ids the current user is a member of; empty when unauthenticated.</summary>
    Task<IReadOnlyList<Guid>> MyWorkspaceIdsAsync(CancellationToken cancellationToken);
}
