namespace Liturgy.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
