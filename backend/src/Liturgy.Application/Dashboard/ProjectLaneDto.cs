namespace Liturgy.Application.Dashboard;

/// <summary>A single project's row in the dashboard's 4D lanes. CurrentPhase is the phase enum's name; the client groups lanes by it.</summary>
public record ProjectLaneDto(Guid Id, string Name, string CurrentPhase, string Meta, bool Blocked);
