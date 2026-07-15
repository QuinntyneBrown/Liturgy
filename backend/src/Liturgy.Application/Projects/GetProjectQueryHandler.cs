using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Projects;

public class GetProjectQueryHandler : IRequestHandler<GetProjectQuery, ProjectJourneyDto>
{
    private readonly IAppDbContext _db;
    private readonly IWorkspaceAccess _workspaceAccess;

    public GetProjectQueryHandler(IAppDbContext db, IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<ProjectJourneyDto> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
            ?? throw new ProjectNotFoundException(request.ProjectId);

        await _workspaceAccess.EnsureProjectVisibleAsync(project.Id, cancellationToken);

        var phases = await _db.Phases
            .AsNoTracking()
            .Where(p => p.ProjectId == project.Id)
            .OrderBy(p => p.Order)
            .ToListAsync(cancellationToken);

        var phaseIds = phases.Select(p => p.Id).ToList();
        var gates = await _db.Gates
            .AsNoTracking()
            .Where(g => phaseIds.Contains(g.PhaseId))
            .ToListAsync(cancellationToken);

        var gateIds = gates.Select(g => g.Id).ToList();
        var requirements = await _db.Requirements
            .AsNoTracking()
            .Where(r => gateIds.Contains(r.GateId))
            .ToListAsync(cancellationToken);

        var phaseDtos = phases
            .Select(phase =>
            {
                var gate = gates.FirstOrDefault(g => g.PhaseId == phase.Id);
                GateDto? gateDto = gate is null
                    ? null
                    : GateDto.From(gate, requirements.Where(r => r.GateId == gate.Id));
                return new PhaseDto(phase.Id, phase.Kind, phase.State, phase.Order, gateDto);
            })
            .ToList();

        return new ProjectJourneyDto(project.Id, project.Name, project.Tag, project.CurrentPhase, phaseDtos);
    }
}
