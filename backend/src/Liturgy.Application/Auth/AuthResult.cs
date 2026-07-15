namespace Liturgy.Application.Auth;

public record AuthResult(
    string AccessToken,
    Guid UserId,
    string Email,
    string Role,
    string FirstName,
    string LastName,
    string Initials);
