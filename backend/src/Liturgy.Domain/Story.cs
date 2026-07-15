namespace Liturgy.Domain;

public class Story
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public int Week { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
