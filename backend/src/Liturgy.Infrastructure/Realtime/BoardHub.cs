using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Liturgy.Infrastructure.Realtime;

/// <summary>
/// Real-time collaboration hub. Clients join a per-project group to receive board,
/// loop, and gate updates as other members act. Server → client events are sent by
/// <see cref="SignalRRealtimeNotifier"/>.
/// </summary>
[Authorize]
public class BoardHub : Hub
{
    public static string ProjectGroup(Guid projectId) => $"project:{projectId}";

    public Task JoinProject(Guid projectId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, ProjectGroup(projectId));

    public Task LeaveProject(Guid projectId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, ProjectGroup(projectId));
}
