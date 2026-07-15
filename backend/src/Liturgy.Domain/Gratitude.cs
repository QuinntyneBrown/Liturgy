namespace Liturgy.Domain;

public class Gratitude
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Quote { get; set; } = string.Empty;
    public string Attribution { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
