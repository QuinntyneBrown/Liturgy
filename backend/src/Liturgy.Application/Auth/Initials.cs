namespace Liturgy.Application.Auth;

public static class Initials
{
    public static string From(string firstName, string lastName)
    {
        var first = string.IsNullOrWhiteSpace(firstName) ? "" : firstName.Trim()[..1];
        var last = string.IsNullOrWhiteSpace(lastName) ? "" : lastName.Trim()[..1];
        var initials = (first + last).ToUpperInvariant();
        return initials.Length == 0 ? "?" : initials;
    }
}
