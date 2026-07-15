using Liturgy.Application.Board;
using Liturgy.Application.Gates;
using Liturgy.Application.Loop;
using Liturgy.Application.Projects;

namespace Liturgy.Application.Abstractions;

/// <summary>
/// Transport-agnostic seam the Infrastructure/SignalR layer implements so command
/// handlers can broadcast collaborative state changes without depending on SignalR.
/// All events are scoped to a single project group.
/// </summary>
public interface IRealtimeNotifier
{
    Task CardMovedAsync(Guid projectId, CardDto card, CancellationToken cancellationToken);
    Task CardCreatedAsync(Guid projectId, CardDto card, CancellationToken cancellationToken);
    Task CardAssignedAsync(Guid projectId, CardDto card, CancellationToken cancellationToken);
    Task MovementLoggedAsync(Guid projectId, CardLoopDto loop, CancellationToken cancellationToken);
    Task RequirementToggledAsync(Guid projectId, GateDto gate, CancellationToken cancellationToken);
    Task GateChangedAsync(Guid projectId, GateDto gate, CancellationToken cancellationToken);
    Task PhaseUnlockedAsync(Guid projectId, PhaseDto phase, CancellationToken cancellationToken);
}
