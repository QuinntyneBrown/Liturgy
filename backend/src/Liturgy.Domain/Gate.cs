namespace Liturgy.Domain;

public class Gate
{
    public Guid Id { get; set; }
    public Guid PhaseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public GateState State { get; set; } = GateState.Blocked;
    public DateTimeOffset CreatedAt { get; set; }
}
