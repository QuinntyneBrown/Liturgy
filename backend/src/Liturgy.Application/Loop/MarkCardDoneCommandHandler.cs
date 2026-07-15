using Liturgy.Application.Abstractions;
using Liturgy.Application.Board;
using Liturgy.Application.Enforcement;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Loop;

public class MarkCardDoneCommandHandler : IRequestHandler<MarkCardDoneCommand, CardDto>
{
    private readonly IAppDbContext _db;
    private readonly EnforcementEngine _engine;
    private readonly IRealtimeNotifier _realtime;
    private readonly IWorkspaceAccess _workspaceAccess;

    public MarkCardDoneCommandHandler(IAppDbContext db, EnforcementEngine engine, IRealtimeNotifier realtime, IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _engine = engine;
        _realtime = realtime;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<CardDto> Handle(MarkCardDoneCommand request, CancellationToken cancellationToken)
    {
        var card = await _db.Cards.FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken)
            ?? throw new CardNotFoundException(request.CardId);

        await _workspaceAccess.EnsureProjectVisibleAsync(card.ProjectId, cancellationToken);

        var movements = await _db.RMovements
            .Where(m => m.CardId == card.Id)
            .ToListAsync(cancellationToken);

        if (!_engine.AllMovementsLogged(movements))
        {
            throw new MovementsIncompleteException(_engine.LoggedCount(movements), EnforcementEngine.Loop.Count);
        }

        card.Column = BoardColumn.Done;
        card.CurrentR = null;
        await _db.SaveChangesAsync(cancellationToken);

        var dto = await CardMapper.ToDtoAsync(_db, card, cancellationToken);
        await _realtime.CardMovedAsync(card.ProjectId, dto, cancellationToken);
        return dto;
    }
}
