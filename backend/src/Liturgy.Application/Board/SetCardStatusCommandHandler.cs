using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Board;

public class SetCardStatusCommandHandler : IRequestHandler<SetCardStatusCommand, CardDto>
{
    private readonly IAppDbContext _db;
    private readonly IRealtimeNotifier _realtime;
    private readonly IWorkspaceAccess _workspaceAccess;

    public SetCardStatusCommandHandler(IAppDbContext db, IRealtimeNotifier realtime, IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _realtime = realtime;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<CardDto> Handle(SetCardStatusCommand request, CancellationToken cancellationToken)
    {
        var card = await _db.Cards.FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken)
            ?? throw new CardNotFoundException(request.CardId);

        await _workspaceAccess.EnsureProjectVisibleAsync(card.ProjectId, cancellationToken);

        card.Status = request.Status;
        await _db.SaveChangesAsync(cancellationToken);

        var dto = await CardMapper.ToDtoAsync(_db, card, cancellationToken);
        // Clients upsert the card when it is Open, and drop it from the board otherwise.
        await _realtime.CardUpdatedAsync(card.ProjectId, dto, cancellationToken);
        return dto;
    }
}
