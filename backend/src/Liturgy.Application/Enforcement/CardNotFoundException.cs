namespace Liturgy.Application.Enforcement;

public class CardNotFoundException : Exception
{
    public CardNotFoundException(Guid id) : base($"Card '{id}' was not found.")
    {
        Id = id;
    }

    public Guid Id { get; }
}
