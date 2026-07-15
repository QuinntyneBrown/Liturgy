using Liturgy.Domain;

namespace Liturgy.Application.Loop;

/// <summary>Thrown when a movement is logged that is not the card's current R in loop order.</summary>
public class OutOfOrderMovementException : Exception
{
    public OutOfOrderMovementException(RKind attempted, RKind? expected)
        : base($"Cannot log '{attempted}': the current movement is '{expected?.ToString() ?? "none"}'.")
    {
        Attempted = attempted;
        Expected = expected;
    }

    public RKind Attempted { get; }
    public RKind? Expected { get; }
}
