namespace Liturgy.Application.Invitations;

public class InvitationNotPendingException : Exception
{
    public InvitationNotPendingException() : base("This invitation is no longer pending.")
    {
    }
}
