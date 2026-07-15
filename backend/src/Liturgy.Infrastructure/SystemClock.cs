using Liturgy.Application.Abstractions;

namespace Liturgy.Infrastructure;

public class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
