namespace Liturgy.Api.Controllers;

public record RegisterRequest(string Email, string FirstName, string LastName, string Password, string? InvitationToken);
