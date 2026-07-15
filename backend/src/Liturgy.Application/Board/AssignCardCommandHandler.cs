using Liturgy.Application.Abstractions;
using Liturgy.Application.Enforcement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Board;

public class AssignCardCommandHandler : IRequestHandler<AssignCardCommand, CardDto>
{
    private readonly IAppDbContext _db;
    private readonly IRealtimeNotifier _realtime;
    private readonly IWorkspaceAccess _workspaceAccess;

    public AssignCardCommandHandler(IAppDbContext db, IRealtimeNotifier realtime, IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _realtime = realtime;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<CardDto> Handle(AssignCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _db.Cards.FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken)
            ?? throw new CardNotFoundException(request.CardId);

        await _workspaceAccess.EnsureProjectVisibleAsync(card.ProjectId, cancellationToken);

        card.AssigneeId = request.AssigneeId;
        await _db.SaveChangesAsync(cancellationToken);

        var dto = await CardMapper.ToDtoAsync(_db, card, cancellationToken);
        await _realtime.CardAssignedAsync(card.ProjectId, dto, cancellationToken);
        return dto;
    }
}
