using Liturgy.Domain;

namespace Liturgy.Application.Enforcement;

/// <summary>
/// The server-authoritative enforcement rules for the FaithTech Playbook. Pure — no
/// EF, no I/O — so every transition rule is cheaply unit-testable. A movement counts
/// as logged when <see cref="RMovement.LoggedAt"/> is set; all derived state (movement
/// states, the card's current R, gate state) is computed from that source of truth.
/// </summary>
public class EnforcementEngine
{
    /// <summary>The five R's in loop order.</summary>
    public static readonly IReadOnlyList<RKind> Loop = new[]
    {
        RKind.Request,
        RKind.Receive,
        RKind.Review,
        RKind.Render,
        RKind.Rejoice
    };

    /// <summary>A card may enter Done only once every R movement has been logged.</summary>
    public bool AllMovementsLogged(IEnumerable<RMovement> movements) =>
        movements.Count(m => m.LoggedAt.HasValue) >= Loop.Count;

    public int LoggedCount(IEnumerable<RMovement> movements) =>
        movements.Count(m => m.LoggedAt.HasValue);

    /// <summary>The next R to work on: the first movement in loop order not yet logged, or null when the loop is complete.</summary>
    public RKind? NextR(IEnumerable<RMovement> movements)
    {
        var logged = movements.Where(m => m.LoggedAt.HasValue).Select(m => m.Kind).ToHashSet();
        foreach (var r in Loop)
        {
            if (!logged.Contains(r))
            {
                return r;
            }
        }

        return null;
    }

    /// <summary>
    /// Recomputes each movement's <see cref="MovementState"/> from its logged status:
    /// logged → Done, the first unlogged → Current, the rest → Locked.
    /// </summary>
    public void RecomputeMovementStates(IEnumerable<RMovement> movements)
    {
        var assignedCurrent = false;
        foreach (var movement in movements.OrderBy(m => m.Order))
        {
            if (movement.LoggedAt.HasValue)
            {
                movement.State = MovementState.Done;
            }
            else if (!assignedCurrent)
            {
                movement.State = MovementState.Current;
                assignedCurrent = true;
            }
            else
            {
                movement.State = MovementState.Locked;
            }
        }
    }

    /// <summary>Guards a board move: any column is reachable except Done, which requires a complete 5R loop.</summary>
    public bool CanEnterColumn(BoardColumn target, IEnumerable<RMovement> movements) =>
        target != BoardColumn.Done || AllMovementsLogged(movements);

    /// <summary>A gate opens only once every requirement on it is Done.</summary>
    public GateState EvaluateGate(IEnumerable<Requirement> requirements) =>
        requirements.All(r => r.State == RequirementState.Done) ? GateState.Open : GateState.Blocked;
}
