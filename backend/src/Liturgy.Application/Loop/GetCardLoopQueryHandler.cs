using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Loop;

public class GetCardLoopQueryHandler : IRequestHandler<GetCardLoopQuery, CardLoopDto>
{
    private readonly IAppDbContext _db;
    private readonly EnforcementEngine _engine;

    public GetCardLoopQueryHandler(IAppDbContext db, EnforcementEngine engine)
    {
        _db = db;
        _engine = engine;
    }

    public async Task<CardLoopDto> Handle(GetCardLoopQuery request, CancellationToken cancellationToken)
    {
        var card = await _db.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken)
            ?? throw new CardNotFoundException(request.CardId);

        var movements = await _db.RMovements
            .AsNoTracking()
            .Where(m => m.CardId == card.Id)
            .ToListAsync(cancellationToken);

        return LoopMapper.ToDto(_engine, card, movements);
    }
}
