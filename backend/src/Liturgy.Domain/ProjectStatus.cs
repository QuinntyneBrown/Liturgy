namespace Liturgy.Domain;

/// <summary>
/// Lifecycle status of a <see cref="Project"/>. A Closed project is soft-hidden from the
/// default project list but not deleted — it can be reopened. Deletion is a separate,
/// permanent operation.
/// </summary>
public enum ProjectStatus
{
    Active = 0,
    Closed = 1
}
