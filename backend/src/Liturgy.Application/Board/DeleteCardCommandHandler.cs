using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Board;

public class DeleteCardCommandHandler : IRequestHandler<DeleteCardCommand>
{
    private readonly IAppDbContext _db;
    private readonly IRealtimeNotifier _realtime;
    private readonly IWorkspaceAccess _workspaceAccess;

    public DeleteCardCommandHandler(IAppDbContext db, IRealtimeNotifier realtime, IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _realtime = realtime;
        _workspaceAccess = workspaceAccess;
    }

    public async Task Handle(DeleteCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _db.Cards.FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken)
            ?? throw new CardNotFoundException(request.CardId);

        await _workspaceAccess.EnsureProjectVisibleAsync(card.ProjectId, cancellationToken);

        var movements = await _db.RMovements.Where(m => m.CardId == card.Id).ToListAsync(cancellationToken);
        _db.RMovements.RemoveRange(movements);
        _db.Cards.Remove(card);
        await _db.SaveChangesAsync(cancellationToken);

        await _realtime.CardDeletedAsync(card.ProjectId, card.Id, cancellationToken);
    }
}
