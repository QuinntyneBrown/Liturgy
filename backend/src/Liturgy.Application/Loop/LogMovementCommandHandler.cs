using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Loop;

public class LogMovementCommandHandler : IRequestHandler<LogMovementCommand, CardLoopDto>
{
    private readonly IAppDbContext _db;
    private readonly EnforcementEngine _engine;
    private readonly IClock _clock;
    private readonly IRealtimeNotifier _realtime;
    private readonly IWorkspaceAccess _workspaceAccess;

    public LogMovementCommandHandler(
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

    public async Task<CardLoopDto> Handle(LogMovementCommand request, CancellationToken cancellationToken)
    {
        var card = await _db.Cards.FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken)
            ?? throw new CardNotFoundException(request.CardId);

        await _workspaceAccess.EnsureProjectVisibleAsync(card.ProjectId, cancellationToken);

        var movements = await _db.RMovements
            .Where(m => m.CardId == card.Id)
            .ToListAsync(cancellationToken);

        // Enforce loop order: only the current (next unlogged) R may be logged.
        var expected = _engine.NextR(movements);
        if (expected != request.Kind)
        {
            throw new OutOfOrderMovementException(request.Kind, expected);
        }

        var movement = movements.Single(m => m.Kind == request.Kind);
        movement.Ask = request.Ask ?? movement.Ask;
        movement.Received = request.Received ?? movement.Received;
        movement.Synthesis = request.Synthesis ?? movement.Synthesis;
        movement.ArtifactUrl = request.ArtifactUrl ?? movement.ArtifactUrl;
        movement.WhatChanged = request.WhatChanged ?? movement.WhatChanged;
        movement.Thanksgiving = request.Thanksgiving ?? movement.Thanksgiving;
        movement.LoggedAt = _clock.UtcNow;

        _engine.RecomputeMovementStates(movements);
        card.CurrentR = _engine.NextR(movements);

        // Logging the first movement pulls the card into the active loop column.
        if (card.Column == BoardColumn.Backlog)
        {
            card.Column = BoardColumn.InLoop;
        }

        await _db.SaveChangesAsync(cancellationToken);

        var dto = LoopMapper.ToDto(_engine, card, movements);
        await _realtime.MovementLoggedAsync(card.ProjectId, dto, cancellationToken);
        return dto;
    }
}
