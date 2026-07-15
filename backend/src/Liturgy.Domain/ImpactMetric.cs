namespace Liturgy.Domain;

public class ImpactMetric
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string Label { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool Highlight { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
