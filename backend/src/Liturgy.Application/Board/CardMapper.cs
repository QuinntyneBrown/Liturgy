using Liturgy.Application.Abstractions;
using Liturgy.Application.Auth;
using Liturgy.Domain;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Board;

/// <summary>Builds <see cref="CardDto"/>s, resolving assignee initials and the logged 5R count.</summary>
public static class CardMapper
{
    public static async Task<CardDto> ToDtoAsync(IAppDbContext db, Card card, CancellationToken cancellationToken)
    {
        var initials = await ResolveInitialsAsync(db, card.AssigneeId, cancellationToken);
        var loggedCount = await db.RMovements
            .Where(m => m.CardId == card.Id && m.LoggedAt != null)
            .CountAsync(cancellationToken);
        return ToDto(card, initials, loggedCount);
    }

    public static CardDto ToDto(Card card, string? assigneeInitials, int loggedCount) => new(
        card.Id,
        card.ProjectId,
        card.SprintId,
        card.Code,
        card.Title,
        card.AssigneeId,
        assigneeInitials,
        card.Column,
        card.CurrentR,
        card.IsBlocked,
        loggedCount);

    public static async Task<string?> ResolveInitialsAsync(IAppDbContext db, Guid? userId, CancellationToken cancellationToken)
    {
        if (userId is null)
        {
            return null;
        }

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return user is null ? null : Initials.From(user.FirstName, user.LastName);
    }
}
