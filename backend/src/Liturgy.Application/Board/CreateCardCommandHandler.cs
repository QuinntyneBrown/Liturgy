using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Board;

public class CreateCardCommandHandler : IRequestHandler<CreateCardCommand, CardDto>
{
    private readonly IAppDbContext _db;
    private readonly EnforcementEngine _engine;
    private readonly IClock _clock;
    private readonly IRealtimeNotifier _realtime;
    private readonly IWorkspaceAccess _workspaceAccess;

    public CreateCardCommandHandler(
        IAppDbContext db,
        EnforcementEngine engine,
        IClock clock,
        IRealtimeNotifier realtime,
        IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _engine = engine;
        _clock = clock;
        _realtime = realtime;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<CardDto> Handle(CreateCardCommand request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
            ?? throw new ProjectNotFoundException(request.ProjectId);

        await _workspaceAccess.EnsureProjectVisibleAsync(project.Id, cancellationToken);

        var sprint = await _db.Sprints
            .Where(s => s.ProjectId == project.Id)
            .OrderByDescending(s => s.Number)
            .FirstOrDefaultAsync(cancellationToken);

        var prefix = CardCode.Prefix(project.Name);
        var existingCodes = await _db.Cards
            .Where(c => c.ProjectId == project.Id)
            .Select(c => c.Code)
            .ToListAsync(cancellationToken);
        var code = CardCode.Next(prefix, existingCodes);

        var now = _clock.UtcNow;
        var card = new Card
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            SprintId = sprint?.Id ?? Guid.Empty,
            Code = code,
            Title = request.Title.Trim(),
            AssigneeId = request.AssigneeId,
            Column = BoardColumn.Backlog,
            CurrentR = RKind.Request,
            IsBlocked = false,
            CreatedAt = now
        };
        _db.Cards.Add(card);

        var order = 1;
        var movements = new List<RMovement>();
        foreach (var kind in EnforcementEngine.Loop)
        {
            var movement = new RMovement
            {
                Id = Guid.NewGuid(),
                CardId = card.Id,
                Kind = kind,
                Order = order++,
                State = MovementState.Locked,
                CreatedAt = now
            };
            movements.Add(movement);
            _db.RMovements.Add(movement);
        }

        // No movements are logged yet, so this makes the first R (Request) current.
        _engine.RecomputeMovementStates(movements);

        await _db.SaveChangesAsync(cancellationToken);

        var dto = await CardMapper.ToDtoAsync(_db, card, cancellationToken);
        await _realtime.CardCreatedAsync(card.ProjectId, dto, cancellationToken);
        return dto;
    }
}
