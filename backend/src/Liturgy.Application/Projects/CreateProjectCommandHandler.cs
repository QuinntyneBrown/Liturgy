using FluentValidation;
using FluentValidation.Results;
using Liturgy.Application.Abstractions;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Projects;

/// <summary>
/// Scaffolds a new project in the caller's first workspace: the standard 4D phase
/// ladder (Discover current, the rest Locked), a Blocked gate with starter
/// requirements on each of Discover/Discern/Develop, and an initial Sprint 1.
/// </summary>
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectSummaryDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public CreateProjectCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<ProjectSummaryDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var workspaceId = _currentUser.UserId is { } userId
            ? await _db.Memberships
                .AsNoTracking()
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => (Guid?)m.WorkspaceId)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        if (workspaceId is null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request), "You must belong to a workspace before creating a project.")
            });
        }

        var now = _clock.UtcNow;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId.Value,
            Name = request.Name.Trim(),
            Tag = request.Tag.Trim(),
            CurrentPhase = PhaseKind.Discover,
            CreatedAt = now
        };
        _db.Projects.Add(project);

        var discover = AddPhase(project.Id, PhaseKind.Discover, PhaseState.Current, 0, now);
        var discern = AddPhase(project.Id, PhaseKind.Discern, PhaseState.Locked, 1, now);
        var develop = AddPhase(project.Id, PhaseKind.Develop, PhaseState.Locked, 2, now);
        AddPhase(project.Id, PhaseKind.Demonstrate, PhaseState.Locked, 3, now);

        AddBlockedGate(discover.Id, "Discover → Discern", now,
            "Lament recorded", "Community interviews synthesized");
        AddBlockedGate(discern.Id, "Discern → Develop", now,
            "Discernment path chosen", "Sprint goal defined for Develop");
        AddBlockedGate(develop.Id, "Develop → Demonstrate", now,
            "Every work item completed the 5R loop", "Impact reframed as friendship compounded by time");

        _db.Sprints.Add(new Sprint
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Number = 1,
            EndsAt = now.AddDays(14),
            CreatedAt = now
        });

        await _db.SaveChangesAsync(cancellationToken);

        return new ProjectSummaryDto(project.Id, project.Name, project.Tag, project.CurrentPhase, project.Status);
    }

    private Phase AddPhase(Guid projectId, PhaseKind kind, PhaseState state, int order, DateTimeOffset now)
    {
        var phase = new Phase
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Kind = kind,
            State = state,
            Order = order,
            CreatedAt = now
        };
        _db.Phases.Add(phase);
        return phase;
    }

    private void AddBlockedGate(Guid phaseId, string title, DateTimeOffset now, params string[] requirementLabels)
    {
        var gate = new Gate
        {
            Id = Guid.NewGuid(),
            PhaseId = phaseId,
            Title = title,
            State = GateState.Blocked,
            CreatedAt = now
        };
        _db.Gates.Add(gate);

        var order = 0;
        foreach (var label in requirementLabels)
        {
            _db.Requirements.Add(new Requirement
            {
                Id = Guid.NewGuid(),
                GateId = gate.Id,
                Label = label,
                State = RequirementState.Todo,
                Order = order++,
                CreatedAt = now
            });
        }
    }
}
