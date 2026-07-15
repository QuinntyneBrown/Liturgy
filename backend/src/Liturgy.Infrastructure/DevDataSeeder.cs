using Liturgy.Application.Abstractions;
using Liturgy.Application.Auth;
using Liturgy.Application.Enforcement;
using Liturgy.Domain;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Infrastructure;

/// <summary>
/// Seeds the "Lantern" demo from docs/mocks so a fresh database mirrors the mockups:
/// the New Hope Collective workspace, four members, a project in Develop with a live
/// Sprint 6 board, and a Develop → Demonstrate gate that is one requirement short of
/// opening (toggle it to watch Demonstrate unlock). Idempotent.
/// </summary>
public class DevDataSeeder
{
    private const string DemoPassword = "Liturgy!2026";

    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IClock _clock;
    private readonly EnforcementEngine _engine;

    public DevDataSeeder(AppDbContext db, IPasswordHasher hasher, IClock clock, EnforcementEngine engine)
    {
        _db = db;
        _hasher = hasher;
        _clock = clock;
        _engine = engine;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.Workspaces.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = _clock.UtcNow;

        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = "New Hope Collective",
            Slug = "new-hope-collective",
            CreatedAt = now
        };
        _db.Workspaces.Add(workspace);

        var members = new[]
        {
            CreateMember(workspace.Id, "Quinn", "Brown", "quinn@newhope.dev", "Lead", now),
            CreateMember(workspace.Id, "Amara", "Mensah", "amara@newhope.dev", "Designer", now),
            CreateMember(workspace.Id, "Jonah", "Park", "jonah@newhope.dev", "Developer", now),
            CreateMember(workspace.Id, "Sam", "Doyle", "sam@newhope.dev", "Developer", now)
        };
        var (quinn, amara, jonah, sam) = (members[0], members[1], members[2], members[3]);

        var project = new Project
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            Name = "Lantern",
            Tag = "After-hours crisis line",
            CurrentPhase = PhaseKind.Develop,
            CreatedAt = now
        };
        _db.Projects.Add(project);

        SeedPhasesAndGates(project.Id, now);

        var sprint = new Sprint
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Number = 6,
            EndsAt = now.AddDays(4),
            CreatedAt = now
        };
        _db.Sprints.Add(sprint);

        SeedCards(project.Id, sprint.Id, quinn.user.Id, amara.user.Id, jonah.user.Id, sam.user.Id, now);

        await _db.SaveChangesAsync(cancellationToken);
    }

    private (User user, Membership membership) CreateMember(
        Guid workspaceId, string first, string last, string email, string role, DateTimeOffset now)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = first,
            LastName = last,
            PasswordHash = _hasher.Hash(DemoPassword),
            Role = "Member",
            CreatedAt = now
        };
        _db.Users.Add(user);

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            UserId = user.Id,
            Role = role,
            Initials = Initials.From(first, last),
            CreatedAt = now
        };
        _db.Memberships.Add(membership);

        return (user, membership);
    }

    private void SeedPhasesAndGates(Guid projectId, DateTimeOffset now)
    {
        // Discover & Discern complete; Develop is the current phase (live board);
        // Demonstrate is locked behind the Develop → Demonstrate gate.
        var discover = AddPhase(projectId, PhaseKind.Discover, PhaseState.Done, 0, now);
        var discern = AddPhase(projectId, PhaseKind.Discern, PhaseState.Done, 1, now);
        var develop = AddPhase(projectId, PhaseKind.Develop, PhaseState.Current, 2, now);
        AddPhase(projectId, PhaseKind.Demonstrate, PhaseState.Locked, 3, now);

        AddOpenGate(discover.Id, "Discover → Discern", now,
            ("Lament recorded — who carries this problem?", "3 entries"),
            ("Community interviews synthesized", "9 people"),
            ("Problem framed with a Christ-shaped lens", "signed off"));

        AddOpenGate(discern.Id, "Discern → Develop", now,
            ("Discernment path chosen", "Reimagine"),
            ("Prayed over with the whole team", "11 July"),
            ("Sprint goal defined for Develop", "done"));

        // The live gate: every requirement done except the last, so a single toggle opens it.
        AddGate(develop.Id, "Develop → Demonstrate", GateState.Blocked, now,
            (RequirementState.Done, "Every work item has completed the 5R loop", "5 of 7"),
            (RequirementState.Done, "Impact reframed as friendship compounded by time", "draft"),
            (RequirementState.Todo, "Demo prepared for the community", "required"));
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

    private void AddOpenGate(Guid phaseId, string title, DateTimeOffset now, params (string Label, string Meta)[] requirements)
    {
        AddGate(phaseId, title, GateState.Open, now,
            requirements.Select(r => (RequirementState.Done, r.Label, r.Meta)).ToArray());
    }

    private void AddGate(
        Guid phaseId,
        string title,
        GateState state,
        DateTimeOffset now,
        params (RequirementState State, string Label, string Meta)[] requirements)
    {
        var gate = new Gate
        {
            Id = Guid.NewGuid(),
            PhaseId = phaseId,
            Title = title,
            State = state,
            CreatedAt = now
        };
        _db.Gates.Add(gate);

        var order = 0;
        foreach (var (reqState, label, meta) in requirements)
        {
            _db.Requirements.Add(new Requirement
            {
                Id = Guid.NewGuid(),
                GateId = gate.Id,
                Label = label,
                Meta = meta,
                State = reqState,
                Order = order++,
                CreatedAt = now
            });
        }
    }

    private void SeedCards(
        Guid projectId, Guid sprintId, Guid quinn, Guid amara, Guid jonah, Guid sam, DateTimeOffset now)
    {
        // (code, title, assignee, column, moves logged)
        AddCard("LAN-24", "Warm-handoff script when a caller is in danger", jonah, BoardColumn.InLoop, 3, now, projectId, sprintId);
        AddCard("LAN-31", "Volunteer availability calendar", amara, BoardColumn.InLoop, 1, now, projectId, sprintId);
        AddCard("LAN-33", "Crisis resource directory by region", quinn, BoardColumn.Backlog, 0, now, projectId, sprintId);
        AddCard("LAN-28", "After-call reflection prompt", sam, BoardColumn.Review, 4, now, projectId, sprintId);
        AddCard("LAN-19", "Anonymized call summary export", jonah, BoardColumn.Backlog, 0, now, projectId, sprintId);
        AddCard("LAN-12", "Quiet-hours routing rules", amara, BoardColumn.Done, 5, now, projectId, sprintId);
        AddCard("LAN-08", "Consent copy for first-time callers", quinn, BoardColumn.Done, 5, now, projectId, sprintId);
    }

    private void AddCard(
        string code, string title, Guid assigneeId, BoardColumn column, int loggedMoves,
        DateTimeOffset now, Guid projectId, Guid sprintId)
    {
        var card = new Card
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            SprintId = sprintId,
            Code = code,
            Title = title,
            AssigneeId = assigneeId,
            Column = column,
            IsBlocked = false,
            CreatedAt = now
        };
        _db.Cards.Add(card);

        var movements = new List<RMovement>();
        var order = 1;
        foreach (var kind in EnforcementEngine.Loop)
        {
            var isLogged = order <= loggedMoves;
            movements.Add(new RMovement
            {
                Id = Guid.NewGuid(),
                CardId = card.Id,
                Kind = kind,
                Order = order,
                LoggedAt = isLogged ? now : null,
                Ask = kind == RKind.Request && isLogged ? "Invite the Spirit into this work; name what we're seeking." : null,
                Received = kind == RKind.Receive && isLogged ? "Captured the raw ideas and constraints without editing." : null,
                Synthesis = kind == RKind.Review && isLogged ? "Shaped the notes into a buildable direction." : null,
                ArtifactUrl = kind == RKind.Render && isLogged ? "prs/lantern/handoff-script-v2" : null,
                WhatChanged = kind == RKind.Render && isLogged ? "Simplified the escalation path from the vision." : null,
                Thanksgiving = kind == RKind.Rejoice && isLogged ? "Grateful for the callers this will protect." : null,
                CreatedAt = now
            });
            order++;
        }

        _engine.RecomputeMovementStates(movements);
        card.CurrentR = _engine.NextR(movements);
        _db.RMovements.AddRange(movements);
    }
}
