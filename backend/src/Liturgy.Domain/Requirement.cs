namespace Liturgy.Domain;

public class Requirement
{
    public Guid Id { get; set; }
    public Guid GateId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Meta { get; set; }
    public RequirementState State { get; set; } = RequirementState.Todo;
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
