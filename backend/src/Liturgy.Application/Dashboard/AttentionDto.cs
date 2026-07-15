namespace Liturgy.Application.Dashboard;

/// <summary>One thing on a caller's workspaces that needs a look: a blocked gate ("gate") or a card stalled in its 5R loop ("loop"). ActionTargetId is a ProjectId for "gate" and a CardId for "loop".</summary>
public record AttentionDto(Guid ProjectId, string ProjectName, string Title, string Reason, string ActionKind, Guid ActionTargetId);
