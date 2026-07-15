namespace Liturgy.Domain;

public class Project
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public PhaseKind CurrentPhase { get; set; } = PhaseKind.Discover;
    public DateTimeOffset CreatedAt { get; set; }
}
