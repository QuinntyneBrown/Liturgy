namespace Liturgy.Application.Auth;

public record CurrentUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string Initials);
