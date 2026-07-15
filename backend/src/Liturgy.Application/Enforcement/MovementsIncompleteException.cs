namespace Liturgy.Application.Enforcement;

/// <summary>Thrown when a card is moved to Done before its 5R loop is complete.</summary>
public class MovementsIncompleteException : Exception
{
    public MovementsIncompleteException(int logged, int required)
        : base($"The 5R loop is incomplete: {logged} of {required} movements logged.")
    {
        Logged = logged;
        Required = required;
    }

    public int Logged { get; }
    public int Required { get; }
}
