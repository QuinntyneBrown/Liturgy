using Liturgy.Application.Abstractions;
using Liturgy.Application.Auth;
using Liturgy.Application.Enforcement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Board;

public class GetBoardQueryHandler : IRequestHandler<GetBoardQuery, BoardDto>
{
    private readonly IAppDbContext _db;

    public GetBoardQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<BoardDto> Handle(GetBoardQuery request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
            ?? throw new ProjectNotFoundException(request.ProjectId);

        var sprint = await _db.Sprints
            .AsNoTracking()
            .Where(s => s.ProjectId == project.Id)
            .OrderByDescending(s => s.Number)
            .FirstOrDefaultAsync(cancellationToken);

        var cards = await _db.Cards
            .AsNoTracking()
            .Where(c => c.ProjectId == project.Id)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var cardIds = cards.Select(c => c.Id).ToList();
        var loggedCounts = await _db.RMovements
            .AsNoTracking()
            .Where(m => cardIds.Contains(m.CardId) && m.LoggedAt != null)
            .GroupBy(m => m.CardId)
            .Select(g => new { CardId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CardId, x => x.Count, cancellationToken);

        var assigneeIds = cards.Where(c => c.AssigneeId != null).Select(c => c.AssigneeId!.Value).Distinct().ToList();
        var initials = await _db.Users
            .AsNoTracking()
            .Where(u => assigneeIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => Initials.From(u.FirstName, u.LastName), cancellationToken);

        var cardDtos = cards
            .Select(c => CardMapper.ToDto(
                c,
                c.AssigneeId is not null && initials.TryGetValue(c.AssigneeId.Value, out var i) ? i : null,
                loggedCounts.TryGetValue(c.Id, out var count) ? count : 0))
            .ToList();

        return new BoardDto(
            project.Id,
            sprint?.Id ?? Guid.Empty,
            sprint?.Number ?? 0,
            cardDtos);
    }
}
