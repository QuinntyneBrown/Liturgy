namespace Liturgy.Application.Enforcement;

public class ProjectNotFoundException : Exception
{
    public ProjectNotFoundException(Guid id) : base($"Project '{id}' was not found.")
    {
        Id = id;
    }

    public Guid Id { get; }
}
