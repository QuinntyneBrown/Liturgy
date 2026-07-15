using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using Liturgy.Application.Projects;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Gates;

/// <summary>
/// Checks or unchecks a gate requirement, recomputes the gate, and — when the gate
/// opens — marks its phase Done and unlocks the next phase in the 4D cycle. This is
/// the server-authoritative gate mechanic; the client never decides gate state.
/// </summary>
public class ToggleRequirementCommandHandler : IRequestHandler<ToggleRequirementCommand, GateDto>
{
    private readonly IAppDbContext _db;
    private readonly EnforcementEngine _engine;
    private readonly IRealtimeNotifier _realtime;
    private readonly IWorkspaceAccess _workspaceAccess;

    public ToggleRequirementCommandHandler(IAppDbContext db, EnforcementEngine engine, IRealtimeNotifier realtime, IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _engine = engine;
        _realtime = realtime;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<GateDto> Handle(ToggleRequirementCommand request, CancellationToken cancellationToken)
    {
        var requirement = await _db.Requirements.FirstOrDefaultAsync(r => r.Id == request.RequirementId, cancellationToken)
            ?? throw new RequirementNotFoundException(request.RequirementId);

        requirement.State = request.Done ? RequirementState.Done : RequirementState.Todo;

        var gate = await _db.Gates.FirstOrDefaultAsync(g => g.Id == requirement.GateId, cancellationToken)
            ?? throw new RequirementNotFoundException(request.RequirementId);

        var requirements = await _db.Requirements
            .Where(r => r.GateId == gate.Id)
            .ToListAsync(cancellationToken);

        var previousGateState = gate.State;
        gate.State = _engine.EvaluateGate(requirements);

        var phase = await _db.Phases.FirstOrDefaultAsync(p => p.Id == gate.PhaseId, cancellationToken);
        if (phase is not null)
        {
            await _workspaceAccess.EnsureProjectVisibleAsync(phase.ProjectId, cancellationToken);
        }

        Phase? unlockedPhase = null;
        if (phase is not null && gate.State == GateState.Open && previousGateState != GateState.Open)
        {
            phase.State = PhaseState.Done;

            unlockedPhase = await _db.Phases
                .Where(p => p.ProjectId == phase.ProjectId && p.Order == phase.Order + 1)
                .FirstOrDefaultAsync(cancellationToken);

            if (unlockedPhase is not null)
            {
                unlockedPhase.State = PhaseState.Current;

                var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == phase.ProjectId, cancellationToken);
                if (project is not null)
                {
                    project.CurrentPhase = unlockedPhase.Kind;
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        var projectId = phase?.ProjectId ?? Guid.Empty;
        var gateDto = GateDto.From(gate, requirements);

        await _realtime.RequirementToggledAsync(projectId, gateDto, cancellationToken);
        if (gate.State != previousGateState)
        {
            await _realtime.GateChangedAsync(projectId, gateDto, cancellationToken);
        }

        if (unlockedPhase is not null)
        {
            var unlockedGate = await _db.Gates.FirstOrDefaultAsync(g => g.PhaseId == unlockedPhase.Id, cancellationToken);
            GateDto? unlockedGateDto = null;
            if (unlockedGate is not null)
            {
                var unlockedRequirements = await _db.Requirements
                    .Where(r => r.GateId == unlockedGate.Id)
                    .ToListAsync(cancellationToken);
                unlockedGateDto = GateDto.From(unlockedGate, unlockedRequirements);
            }

            var phaseDto = new PhaseDto(unlockedPhase.Id, unlockedPhase.Kind, unlockedPhase.State, unlockedPhase.Order, unlockedGateDto);
            await _realtime.PhaseUnlockedAsync(projectId, phaseDto, cancellationToken);
        }

        return gateDto;
    }
}
