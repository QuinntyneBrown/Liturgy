using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Board;

public class MoveCardCommandHandler : IRequestHandler<MoveCardCommand, CardDto>
{
    private readonly IAppDbContext _db;
    private readonly EnforcementEngine _engine;
    private readonly IRealtimeNotifier _realtime;

    public MoveCardCommandHandler(IAppDbContext db, EnforcementEngine engine, IRealtimeNotifier realtime)
    {
        _db = db;
        _engine = engine;
        _realtime = realtime;
    }

    public async Task<CardDto> Handle(MoveCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _db.Cards.FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken)
            ?? throw new CardNotFoundException(request.CardId);

        var movements = await _db.RMovements
            .Where(m => m.CardId == card.Id)
            .ToListAsync(cancellationToken);

        if (!_engine.CanEnterColumn(request.TargetColumn, movements))
        {
            throw new MovementsIncompleteException(_engine.LoggedCount(movements), EnforcementEngine.Loop.Count);
        }

        card.Column = request.TargetColumn;
        if (request.TargetColumn == BoardColumn.Done)
        {
            card.CurrentR = null;
        }

        await _db.SaveChangesAsync(cancellationToken);

        var dto = await CardMapper.ToDtoAsync(_db, card, cancellationToken);
        await _realtime.CardMovedAsync(card.ProjectId, dto, cancellationToken);
        return dto;
    }
}
