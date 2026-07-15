using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Projects;

/// <summary>
/// Hard-deletes a project together with every entity that hangs off it. No foreign-key
/// cascades are configured, so each dependent set is removed explicitly, child-first, in a
/// single transaction.
/// </summary>
public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
{
    private readonly IAppDbContext _db;
    private readonly IWorkspaceAccess _workspaceAccess;

    public DeleteProjectCommandHandler(IAppDbContext db, IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
    }

    public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        await _workspaceAccess.EnsureProjectVisibleAsync(request.Id, cancellationToken);

        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new ProjectNotFoundException(request.Id);

        var cards = await _db.Cards.Where(c => c.ProjectId == project.Id).ToListAsync(cancellationToken);
        var cardIds = cards.Select(c => c.Id).ToList();
        var movements = await _db.RMovements.Where(m => cardIds.Contains(m.CardId)).ToListAsync(cancellationToken);

        var phases = await _db.Phases.Where(p => p.ProjectId == project.Id).ToListAsync(cancellationToken);
        var phaseIds = phases.Select(p => p.Id).ToList();
        var gates = await _db.Gates.Where(g => phaseIds.Contains(g.PhaseId)).ToListAsync(cancellationToken);
        var gateIds = gates.Select(g => g.Id).ToList();
        var requirements = await _db.Requirements.Where(r => gateIds.Contains(r.GateId)).ToListAsync(cancellationToken);

        var sprints = await _db.Sprints.Where(s => s.ProjectId == project.Id).ToListAsync(cancellationToken);
        var decisions = await _db.Decisions.Where(d => d.ProjectId == project.Id).ToListAsync(cancellationToken);
        var metrics = await _db.ImpactMetrics.Where(m => m.ProjectId == project.Id).ToListAsync(cancellationToken);
        var stories = await _db.Stories.Where(s => s.ProjectId == project.Id).ToListAsync(cancellationToken);
        var gratitudes = await _db.Gratitudes.Where(g => g.ProjectId == project.Id).ToListAsync(cancellationToken);

        _db.RMovements.RemoveRange(movements);
        _db.Cards.RemoveRange(cards);
        _db.Requirements.RemoveRange(requirements);
        _db.Gates.RemoveRange(gates);
        _db.Phases.RemoveRange(phases);
        _db.Sprints.RemoveRange(sprints);
        _db.Decisions.RemoveRange(decisions);
        _db.ImpactMetrics.RemoveRange(metrics);
        _db.Stories.RemoveRange(stories);
        _db.Gratitudes.RemoveRange(gratitudes);
        _db.Projects.Remove(project);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
