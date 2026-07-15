using Liturgy.Application.Abstractions;
using Liturgy.Application.Board;
using Liturgy.Application.Gates;
using Liturgy.Application.Loop;
using Liturgy.Application.Projects;
using Microsoft.AspNetCore.SignalR;

namespace Liturgy.Infrastructure.Realtime;

public class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<BoardHub> _hub;

    public SignalRRealtimeNotifier(IHubContext<BoardHub> hub)
    {
        _hub = hub;
    }

    public Task CardMovedAsync(Guid projectId, CardDto card, CancellationToken cancellationToken) =>
        Send(projectId, "CardMoved", card, cancellationToken);

    public Task CardCreatedAsync(Guid projectId, CardDto card, CancellationToken cancellationToken) =>
        Send(projectId, "CardCreated", card, cancellationToken);

    public Task CardAssignedAsync(Guid projectId, CardDto card, CancellationToken cancellationToken) =>
        Send(projectId, "CardAssigned", card, cancellationToken);

    public Task CardUpdatedAsync(Guid projectId, CardDto card, CancellationToken cancellationToken) =>
        Send(projectId, "CardUpdated", card, cancellationToken);

    public Task CardDeletedAsync(Guid projectId, Guid cardId, CancellationToken cancellationToken) =>
        Send(projectId, "CardDeleted", cardId, cancellationToken);

    public Task MovementLoggedAsync(Guid projectId, CardLoopDto loop, CancellationToken cancellationToken) =>
        Send(projectId, "MovementLogged", loop, cancellationToken);

    public Task RequirementToggledAsync(Guid projectId, GateDto gate, CancellationToken cancellationToken) =>
        Send(projectId, "RequirementToggled", gate, cancellationToken);

    public Task GateChangedAsync(Guid projectId, GateDto gate, CancellationToken cancellationToken) =>
        Send(projectId, "GateChanged", gate, cancellationToken);

    public Task PhaseUnlockedAsync(Guid projectId, PhaseDto phase, CancellationToken cancellationToken) =>
        Send(projectId, "PhaseUnlocked", phase, cancellationToken);

    private Task Send(Guid projectId, string method, object payload, CancellationToken cancellationToken) =>
        _hub.Clients.Group(BoardHub.ProjectGroup(projectId)).SendAsync(method, payload, cancellationToken);
}
