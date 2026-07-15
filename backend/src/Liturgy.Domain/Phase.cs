namespace Liturgy.Domain;

public class Phase
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public PhaseKind Kind { get; set; }
    public PhaseState State { get; set; } = PhaseState.Locked;
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
