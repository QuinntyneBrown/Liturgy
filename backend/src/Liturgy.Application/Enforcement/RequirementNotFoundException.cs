namespace Liturgy.Application.Enforcement;

public class RequirementNotFoundException : Exception
{
    public RequirementNotFoundException(Guid id) : base($"Requirement '{id}' was not found.")
    {
        Id = id;
    }

    public Guid Id { get; }
}
