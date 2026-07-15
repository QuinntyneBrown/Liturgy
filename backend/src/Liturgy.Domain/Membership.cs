namespace Liturgy.Domain;

public class Membership
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member";
    public string Initials { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
