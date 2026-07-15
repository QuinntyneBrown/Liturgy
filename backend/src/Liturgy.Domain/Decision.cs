namespace Liturgy.Domain;

public class Decision
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public DiscernmentPath ChosenPath { get; set; }
    public string Rationale { get; set; } = string.Empty;
    public string PrayedOverWith { get; set; } = string.Empty;
    public DateTimeOffset DecidedAt { get; set; }
}
