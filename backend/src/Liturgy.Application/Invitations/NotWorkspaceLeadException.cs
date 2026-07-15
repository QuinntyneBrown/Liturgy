namespace Liturgy.Application.Invitations;

/// <summary>Thrown when a non-Lead attempts a Lead-only account operation (invite / revoke).</summary>
public class NotWorkspaceLeadException : Exception
{
    public NotWorkspaceLeadException() : base("Only a workspace Lead can manage invitations.")
    {
    }
}
