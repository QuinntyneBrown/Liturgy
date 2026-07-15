namespace Liturgy.Domain;

public class Card
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid SprintId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid? AssigneeId { get; set; }
    public BoardColumn Column { get; set; } = BoardColumn.Backlog;
    public RKind? CurrentR { get; set; }
    public bool IsBlocked { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
