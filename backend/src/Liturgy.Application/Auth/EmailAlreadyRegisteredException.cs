namespace Liturgy.Application.Auth;

public class EmailAlreadyRegisteredException : Exception
{
    public EmailAlreadyRegisteredException(string email)
        : base($"Email '{email}' is already registered.")
    {
        Email = email;
    }

    public string Email { get; }
}
