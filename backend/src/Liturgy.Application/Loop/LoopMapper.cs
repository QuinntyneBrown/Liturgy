using Liturgy.Application.Enforcement;
using Liturgy.Domain;

namespace Liturgy.Application.Loop;

public static class LoopMapper
{
    public static CardLoopDto ToDto(EnforcementEngine engine, Card card, IReadOnlyCollection<RMovement> movements) => new(
        card.Id,
        card.ProjectId,
        card.Code,
        card.Title,
        card.Column,
        card.CurrentR,
        engine.LoggedCount(movements),
        engine.AllMovementsLogged(movements),
        movements.OrderBy(m => m.Order).Select(MovementDto.From).ToList());
}
