namespace Liturgy.Application.Invitations;

public class InvitationNotFoundException : Exception
{
    public InvitationNotFoundException(string token) : base($"Invitation '{token}' was not found.")
    {
    }
}
