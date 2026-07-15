using Liturgy.Application.Abstractions;
using Liturgy.Application.Gates;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Discernment;

/// <summary>
/// Upserts the project's Discern decision, then satisfies the Discern gate's "path
/// chosen" and "note"/"rationale" requirements by reusing
/// <see cref="ToggleRequirementCommand"/> rather than duplicating gate logic. That
/// keeps gate recomputation, phase-unlock cascade, and realtime broadcast in one
/// place. Any other requirement on the gate (e.g. "Sprint goal defined") is left
/// untouched, so the gate stays blocked until it's satisfied separately.
/// </summary>
public class UpdateDecisionCommandHandler : IRequestHandler<UpdateDecisionCommand, DecisionDto>
{
    private readonly IAppDbContext _db;
    private readonly IWorkspaceAccess _workspaceAccess;
    private readonly IClock _clock;
    private readonly IMediator _mediator;

    public UpdateDecisionCommandHandler(IAppDbContext db, IWorkspaceAccess workspaceAccess, IClock clock, IMediator mediator)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
        _clock = clock;
        _mediator = mediator;
    }

    public async Task<DecisionDto> Handle(UpdateDecisionCommand request, CancellationToken cancellationToken)
    {
        await _workspaceAccess.EnsureProjectVisibleAsync(request.ProjectId, cancellationToken);

        var decision = await _db.Decisions.FirstOrDefaultAsync(d => d.ProjectId == request.ProjectId, cancellationToken);
        if (decision is null)
        {
            decision = new Decision { Id = Guid.NewGuid(), ProjectId = request.ProjectId };
            _db.Decisions.Add(decision);
        }

        decision.ChosenPath = request.ChosenPath;
        decision.Rationale = request.Rationale;
        decision.PrayedOverWith = request.PrayedOverWith;
        decision.DecidedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        var discernPhase = await _db.Phases
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId && p.Kind == PhaseKind.Discern, cancellationToken);

        if (discernPhase is not null)
        {
            var gate = await _db.Gates.AsNoTracking().FirstOrDefaultAsync(g => g.PhaseId == discernPhase.Id, cancellationToken);
            if (gate is not null)
            {
                var requirements = await _db.Requirements
                    .AsNoTracking()
                    .Where(r => r.GateId == gate.Id)
                    .ToListAsync(cancellationToken);

                var pathRequirement = requirements.FirstOrDefault(r => r.Label.Contains("path", StringComparison.OrdinalIgnoreCase));
                if (pathRequirement is not null)
                {
                    await _mediator.Send(new ToggleRequirementCommand(pathRequirement.Id, true), cancellationToken);
                }

                var noteRequirement = requirements.FirstOrDefault(r =>
                    r.Label.Contains("note", StringComparison.OrdinalIgnoreCase) ||
                    r.Label.Contains("rationale", StringComparison.OrdinalIgnoreCase));
                if (noteRequirement is not null)
                {
                    await _mediator.Send(
                        new ToggleRequirementCommand(noteRequirement.Id, !string.IsNullOrWhiteSpace(request.Rationale)),
                        cancellationToken);
                }
            }
        }

        return DecisionDto.From(decision);
    }
}
