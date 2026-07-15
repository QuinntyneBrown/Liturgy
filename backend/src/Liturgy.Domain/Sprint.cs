namespace Liturgy.Domain;

public class Sprint
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public int Number { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
