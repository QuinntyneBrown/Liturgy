using Liturgy.Application.Abstractions;
using Liturgy.Application.Auth;
using Liturgy.Application.Enforcement;
using Liturgy.Domain;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Infrastructure;

/// <summary>
/// Seeds the New Hope Collective workspace from docs/mocks so a fresh database mirrors
/// the mockups: four members and six projects spread across every stage of the 4D
/// cycle — Lantern (Develop, one gate requirement from opening Demonstrate), Wellspring
/// (Discern, blocked on a sprint goal), Bread & Fish (Demonstrate, full impact story),
/// Refuge Finder and Sabbath (Discover), and Common Table (Develop). Idempotent.
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

    private const string DemoWorkspaceSlug = "new-hope-collective";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Key idempotency on the demo workspace specifically, not on "any workspace" —
        // self-registration creates a private workspace per user, so a general check
        // would skip seeding forever once a single account had signed up.
        if (await _db.Workspaces.AnyAsync(w => w.Slug == DemoWorkspaceSlug, cancellationToken))
        {
            return;
        }

        var now = _clock.UtcNow;

        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = "New Hope Collective",
            Slug = DemoWorkspaceSlug,
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

        SeedWellspring(workspace.Id, now);
        SeedBreadAndFish(workspace.Id, now);
        SeedDiscoveryProject(workspace.Id, "Refuge Finder", "Trauma-informed shelter finder", now);
        SeedDiscoveryProject(workspace.Id, "Sabbath", "Rhythms of rest", now);
        SeedCommonTable(workspace.Id, now);

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

    private void SeedWellspring(Guid workspaceId, DateTimeOffset now)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Name = "Wellspring",
            Tag = "Clean-water access",
            CurrentPhase = PhaseKind.Discern,
            CreatedAt = now
        };
        _db.Projects.Add(project);

        var discover = AddPhase(project.Id, PhaseKind.Discover, PhaseState.Done, 0, now);
        var discern = AddPhase(project.Id, PhaseKind.Discern, PhaseState.Current, 1, now);
        AddPhase(project.Id, PhaseKind.Develop, PhaseState.Locked, 2, now);
        AddPhase(project.Id, PhaseKind.Demonstrate, PhaseState.Locked, 3, now);

        AddOpenGate(discover.Id, "Discover → Discern", now,
            ("Lament recorded — who carries this problem?", "4 entries"),
            ("Community interviews synthesized", "12 people"),
            ("Problem framed with a Christ-shaped lens", "signed off"));

        // Blocked one requirement short: the Decision below satisfies "path chosen" and
        // "note written", so only the sprint goal keeps this gate from opening.
        AddGate(discern.Id, "Discern → Develop", GateState.Blocked, now,
            (RequirementState.Done, "Discernment path chosen", "Reimagine"),
            (RequirementState.Done, "Discernment note written", null),
            (RequirementState.Todo, "Sprint goal defined for Develop", "required"));

        _db.Decisions.Add(new Decision
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            ChosenPath = DiscernmentPath.Reimagine,
            Rationale = "A borehole-monitoring tool already exists but ignores the women who walk for water. " +
                        "We'll reimagine it around their route and their voice.",
            PrayedOverWith = "Whole team · 11 July",
            DecidedAt = now
        });
    }

    private void SeedBreadAndFish(Guid workspaceId, DateTimeOffset now)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Name = "Bread & Fish",
            Tag = "Neighbourhood meal-share",
            CurrentPhase = PhaseKind.Demonstrate,
            CreatedAt = now
        };
        _db.Projects.Add(project);

        var discover = AddPhase(project.Id, PhaseKind.Discover, PhaseState.Done, 0, now);
        var discern = AddPhase(project.Id, PhaseKind.Discern, PhaseState.Done, 1, now);
        var develop = AddPhase(project.Id, PhaseKind.Develop, PhaseState.Done, 2, now);
        AddPhase(project.Id, PhaseKind.Demonstrate, PhaseState.Current, 3, now);

        AddOpenGate(discover.Id, "Discover → Discern", now,
            ("Lament recorded — who carries this problem?", "5 entries"),
            ("Community interviews synthesized", "14 people"),
            ("Problem framed with a Christ-shaped lens", "signed off"));

        AddOpenGate(discern.Id, "Discern → Develop", now,
            ("Discernment path chosen", "Create"),
            ("Prayed over with the whole team", "3 March"),
            ("Sprint goal defined for Develop", "done"));

        AddOpenGate(develop.Id, "Develop → Demonstrate", now,
            ("Every work item has completed the 5R loop", "12 of 12"),
            ("Impact reframed as friendship compounded by time", "draft"),
            ("Demo prepared for the community", "done"));

        var metrics = new (string Value, string? Unit, string Label, bool Highlight)[]
        {
            ("17", " wks", "Weeks walked alongside the same 6 families", false),
            ("41", null, "Meals shared between neighbours who were strangers", false),
            ("9", null, "Volunteers still serving past the pilot", false),
            ("4", null, "Stories of change, in their own words", true)
        };
        var order = 0;
        foreach (var (value, unit, label, highlight) in metrics)
        {
            _db.ImpactMetrics.Add(new ImpactMetric
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Value = value,
                Unit = unit,
                Label = label,
                Order = order++,
                Highlight = highlight,
                CreatedAt = now
            });
        }

        var stories = new (int Week, string Text)[]
        {
            (2, "Maria signed up for a meal. She didn't know a single name on her street."),
            (7, "Maria started cooking on Thursdays. Two neighbours now have her number."),
            (14, "The Thursday table outgrew Maria's kitchen. They moved it to the church hall."),
            (17, "Maria is training two others to host. The app now just quietly keeps the rota.")
        };
        order = 0;
        foreach (var (week, text) in stories)
        {
            _db.Stories.Add(new Story
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Week = week,
                Text = text,
                Order = order++,
                CreatedAt = now
            });
        }

        var gratitude = new (string Quote, string Attribution)[]
        {
            ("We prayed for a way to reach the isolated. He gave us a table, not a feature.", "Team retro, week 12"),
            ("Grateful the rota code was boring so the meals could be beautiful.", "Sam, developer"),
            ("I came for food. I stayed for the people. Thank you for building the door.", "A guest, week 15")
        };
        order = 0;
        foreach (var (quote, attribution) in gratitude)
        {
            _db.Gratitudes.Add(new Gratitude
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Quote = quote,
                Attribution = attribution,
                Order = order++,
                CreatedAt = now
            });
        }
    }

    private void SeedDiscoveryProject(Guid workspaceId, string name, string tag, DateTimeOffset now)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Name = name,
            Tag = tag,
            CurrentPhase = PhaseKind.Discover,
            CreatedAt = now
        };
        _db.Projects.Add(project);

        var discover = AddPhase(project.Id, PhaseKind.Discover, PhaseState.Current, 0, now);
        AddPhase(project.Id, PhaseKind.Discern, PhaseState.Locked, 1, now);
        AddPhase(project.Id, PhaseKind.Develop, PhaseState.Locked, 2, now);
        AddPhase(project.Id, PhaseKind.Demonstrate, PhaseState.Locked, 3, now);

        AddGate(discover.Id, "Discover → Discern", GateState.Blocked, now,
            (RequirementState.Todo, "Lament recorded — who carries this problem?", null),
            (RequirementState.Todo, "Community interviews synthesized", null));
    }

    private void SeedCommonTable(Guid workspaceId, DateTimeOffset now)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Name = "Common Table",
            Tag = "Shared-meal logistics",
            CurrentPhase = PhaseKind.Develop,
            CreatedAt = now
        };
        _db.Projects.Add(project);

        var discover = AddPhase(project.Id, PhaseKind.Discover, PhaseState.Done, 0, now);
        var discern = AddPhase(project.Id, PhaseKind.Discern, PhaseState.Done, 1, now);
        var develop = AddPhase(project.Id, PhaseKind.Develop, PhaseState.Current, 2, now);
        AddPhase(project.Id, PhaseKind.Demonstrate, PhaseState.Locked, 3, now);

        AddOpenGate(discover.Id, "Discover → Discern", now,
            ("Lament recorded — who carries this problem?", "3 entries"),
            ("Community interviews synthesized", "8 people"),
            ("Problem framed with a Christ-shaped lens", "signed off"));

        AddOpenGate(discern.Id, "Discern → Develop", now,
            ("Discernment path chosen", "Receive"),
            ("Prayed over with the whole team", "2 June"),
            ("Sprint goal defined for Develop", "done"));

        AddGate(develop.Id, "Develop → Demonstrate", GateState.Blocked, now,
            (RequirementState.Todo, "Every work item has completed the 5R loop", "0 of 4"),
            (RequirementState.Todo, "Impact reframed as friendship compounded by time", "required"));

        _db.Sprints.Add(new Sprint
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Number = 2,
            EndsAt = now.AddDays(5),
            CreatedAt = now
        });
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
            requirements.Select(r => (RequirementState.Done, r.Label, (string?)r.Meta)).ToArray());
    }

    private void AddGate(
        Guid phaseId,
        string title,
        GateState state,
        DateTimeOffset now,
        params (RequirementState State, string Label, string? Meta)[] requirements)
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
