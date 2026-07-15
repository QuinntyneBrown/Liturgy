namespace Liturgy.Application.Enforcement;

/// <summary>Thrown when an action is attempted behind a gate that is still blocked.</summary>
public class GateLockedException : Exception
{
    public GateLockedException(string title)
        : base($"The gate '{title}' is blocked; complete its requirements first.")
    {
        Title = title;
    }

    public string Title { get; }
}
