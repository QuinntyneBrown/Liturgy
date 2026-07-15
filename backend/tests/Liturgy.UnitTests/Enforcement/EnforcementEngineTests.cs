using Liturgy.Application.Enforcement;
using Liturgy.Domain;
using Xunit;

namespace Liturgy.UnitTests.Enforcement;

public class EnforcementEngineTests
{
    private readonly EnforcementEngine _engine = new();

    private static List<RMovement> Loop(int logged)
    {
        var movements = new List<RMovement>();
        var order = 1;
        foreach (var kind in EnforcementEngine.Loop)
        {
            movements.Add(new RMovement
            {
                Id = Guid.NewGuid(),
                Kind = kind,
                Order = order,
                LoggedAt = order <= logged ? DateTimeOffset.UtcNow : null
            });
            order++;
        }

        return movements;
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(3, false)]
    [InlineData(4, false)]
    [InlineData(5, true)]
    public void AllMovementsLogged_is_true_only_when_all_five_are_logged(int logged, bool expected)
    {
        Assert.Equal(expected, _engine.AllMovementsLogged(Loop(logged)));
    }

    [Fact]
    public void NextR_returns_the_first_unlogged_R_in_loop_order()
    {
        Assert.Equal(RKind.Request, _engine.NextR(Loop(0)));
        Assert.Equal(RKind.Render, _engine.NextR(Loop(3)));
        Assert.Equal(RKind.Rejoice, _engine.NextR(Loop(4)));
    }

    [Fact]
    public void NextR_is_null_when_the_loop_is_complete()
    {
        Assert.Null(_engine.NextR(Loop(5)));
    }

    [Fact]
    public void RecomputeMovementStates_marks_logged_done_first_unlogged_current_rest_locked()
    {
        var movements = Loop(2);

        _engine.RecomputeMovementStates(movements);

        var ordered = movements.OrderBy(m => m.Order).ToList();
        Assert.Equal(MovementState.Done, ordered[0].State);
        Assert.Equal(MovementState.Done, ordered[1].State);
        Assert.Equal(MovementState.Current, ordered[2].State);
        Assert.Equal(MovementState.Locked, ordered[3].State);
        Assert.Equal(MovementState.Locked, ordered[4].State);
    }

    [Fact]
    public void RecomputeMovementStates_leaves_no_current_when_complete()
    {
        var movements = Loop(5);

        _engine.RecomputeMovementStates(movements);

        Assert.All(movements, m => Assert.Equal(MovementState.Done, m.State));
    }

    [Theory]
    [InlineData(BoardColumn.Backlog, 0, true)]
    [InlineData(BoardColumn.InLoop, 2, true)]
    [InlineData(BoardColumn.Review, 4, true)]
    [InlineData(BoardColumn.Done, 4, false)]
    [InlineData(BoardColumn.Done, 5, true)]
    public void CanEnterColumn_blocks_Done_until_the_loop_is_complete(BoardColumn target, int logged, bool expected)
    {
        Assert.Equal(expected, _engine.CanEnterColumn(target, Loop(logged)));
    }

    [Fact]
    public void EvaluateGate_opens_only_when_every_requirement_is_done()
    {
        var requirements = new[]
        {
            new Requirement { State = RequirementState.Done },
            new Requirement { State = RequirementState.Done },
            new Requirement { State = RequirementState.Todo }
        };

        Assert.Equal(GateState.Blocked, _engine.EvaluateGate(requirements));

        requirements[2].State = RequirementState.Done;
        Assert.Equal(GateState.Open, _engine.EvaluateGate(requirements));
    }

    [Fact]
    public void EvaluateGate_opens_when_there_are_no_requirements()
    {
        Assert.Equal(GateState.Open, _engine.EvaluateGate(Array.Empty<Requirement>()));
    }
}
