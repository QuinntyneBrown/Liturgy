using System.Globalization;
using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Dashboard;

/// <summary>
/// Aggregates momentum, attention items, and 4D lanes across every project in the
/// caller's workspaces. Reads only — nothing here mutates state, so it fetches each
/// entity set once and joins in memory rather than re-querying per project.
/// </summary>
public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IAppDbContext _db;
    private readonly IWorkspaceAccess _workspaceAccess;
    private readonly IClock _clock;

    public GetDashboardQueryHandler(IAppDbContext db, IWorkspaceAccess workspaceAccess, IClock clock)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
        _clock = clock;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var workspaceIds = await _workspaceAccess.MyWorkspaceIdsAsync(cancellationToken);

        var projects = await _db.Projects
            .AsNoTracking()
            .Where(p => workspaceIds.Contains(p.WorkspaceId))
            .ToListAsync(cancellationToken);
        var projectIds = projects.Select(p => p.Id).ToList();

        var phases = await _db.Phases.AsNoTracking().Where(p => projectIds.Contains(p.ProjectId)).ToListAsync(cancellationToken);
        var phaseIds = phases.Select(p => p.Id).ToList();

        var gates = await _db.Gates.AsNoTracking().Where(g => phaseIds.Contains(g.PhaseId)).ToListAsync(cancellationToken);
        var gateIds = gates.Select(g => g.Id).ToList();

        var requirements = await _db.Requirements.AsNoTracking().Where(r => gateIds.Contains(r.GateId)).ToListAsync(cancellationToken);

        var sprints = await _db.Sprints.AsNoTracking().Where(s => projectIds.Contains(s.ProjectId)).ToListAsync(cancellationToken);

        var cards = await _db.Cards.AsNoTracking().Where(c => projectIds.Contains(c.ProjectId)).ToListAsync(cancellationToken);
        var cardIds = cards.Select(c => c.Id).ToList();

        var movements = await _db.RMovements.AsNoTracking().Where(m => cardIds.Contains(m.CardId)).ToListAsync(cancellationToken);

        var stories = await _db.Stories.AsNoTracking().Where(s => projectIds.Contains(s.ProjectId)).ToListAsync(cancellationToken);

        var momentum = BuildMomentum(projects, gates, movements, stories);
        var attention = BuildAttention(projects, phases, gates, requirements, cards, movements);
        var lanes = BuildLanes(projects, phases, gates, sprints, cards, movements, stories);

        return new DashboardDto(momentum, attention, lanes);
    }

    private MomentumDto BuildMomentum(
        List<Project> projects, List<Gate> gates, List<RMovement> movements, List<Story> stories)
    {
        var weekAgo = _clock.UtcNow.AddDays(-7);

        return new MomentumDto(
            ActiveProjects: projects.Count,
            MovementsThisWeek: movements.Count(m => m.LoggedAt is { } loggedAt && loggedAt >= weekAgo),
            GatesBlocked: gates.Count(g => g.State == GateState.Blocked),
            WeeksCompounded: stories.Count == 0 ? 0 : stories.Max(s => s.Week));
    }

    private static List<AttentionDto> BuildAttention(
        List<Project> projects,
        List<Phase> phases,
        List<Gate> gates,
        List<Requirement> requirements,
        List<Card> cards,
        List<RMovement> movements)
    {
        var attention = new List<AttentionDto>();

        foreach (var gate in gates.Where(g => g.State == GateState.Blocked))
        {
            var phase = phases.FirstOrDefault(p => p.Id == gate.PhaseId);
            var project = phase is null ? null : projects.FirstOrDefault(p => p.Id == phase.ProjectId);
            if (project is null)
            {
                continue;
            }

            var remaining = requirements.Count(r => r.GateId == gate.Id && r.State == RequirementState.Todo);
            attention.Add(new AttentionDto(
                project.Id,
                project.Name,
                $"{project.Name} · {gate.Title}",
                $"{remaining} requirement(s) remaining before this gate opens.",
                "gate",
                project.Id));
        }

        foreach (var card in cards.Where(c => c.Status == CardStatus.Open && c.Column is BoardColumn.InLoop or BoardColumn.Review))
        {
            var loggedKinds = movements
                .Where(m => m.CardId == card.Id && m.LoggedAt.HasValue)
                .Select(m => m.Kind)
                .ToHashSet();
            if (loggedKinds.Count >= EnforcementEngine.Loop.Count)
            {
                continue;
            }

            var project = projects.FirstOrDefault(p => p.Id == card.ProjectId);
            if (project is null)
            {
                continue;
            }

            var missing = EnforcementEngine.Loop.Where(k => !loggedKinds.Contains(k)).Select(k => k.ToString());
            var remaining = EnforcementEngine.Loop.Count - loggedKinds.Count;

            attention.Add(new AttentionDto(
                project.Id,
                project.Name,
                $"{project.Name} · card {card.Code} can't reach Done",
                $"{string.Join(", ", missing)} not logged. {remaining} of five movements remain.",
                "loop",
                card.Id));
        }

        return attention;
    }

    private static List<ProjectLaneDto> BuildLanes(
        List<Project> projects,
        List<Phase> phases,
        List<Gate> gates,
        List<Sprint> sprints,
        List<Card> cards,
        List<RMovement> movements,
        List<Story> stories)
    {
        return projects.Select(project =>
        {
            var projectPhaseIds = phases.Where(p => p.ProjectId == project.Id).Select(p => p.Id).ToHashSet();
            var blocked = gates.Any(g => projectPhaseIds.Contains(g.PhaseId) && g.State == GateState.Blocked);

            var meta = project.CurrentPhase switch
            {
                PhaseKind.Discover => "In discovery",
                PhaseKind.Discern => blocked ? "Decision pending" : "Discerning",
                PhaseKind.Develop => DevelopMeta(project, sprints, cards, movements),
                PhaseKind.Demonstrate => DemonstrateMeta(project, stories),
                _ => string.Empty
            };

            return new ProjectLaneDto(project.Id, project.Name, project.CurrentPhase.ToString(), meta, blocked);
        }).ToList();
    }

    private static string DevelopMeta(Project project, List<Sprint> sprints, List<Card> cards, List<RMovement> movements)
    {
        var latestSprint = sprints.Where(s => s.ProjectId == project.Id).OrderByDescending(s => s.Number).FirstOrDefault();
        var projectCards = cards.Where(c => c.ProjectId == project.Id && c.Status == CardStatus.Open).ToList();
        var avgLoggedR = projectCards.Count == 0
            ? 0d
            : projectCards.Average(c => movements.Count(m => m.CardId == c.Id && m.LoggedAt.HasValue));

        return $"Sprint {latestSprint?.Number ?? 0} · {avgLoggedR.ToString("0.0", CultureInfo.InvariantCulture)}/5 R avg";
    }

    private static string DemonstrateMeta(Project project, List<Story> stories)
    {
        var projectStories = stories.Where(s => s.ProjectId == project.Id).ToList();
        var maxWeek = projectStories.Count == 0 ? 0 : projectStories.Max(s => s.Week);
        return $"{maxWeek} weeks · {projectStories.Count} stories";
    }
}
