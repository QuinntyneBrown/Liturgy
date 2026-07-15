namespace Liturgy.Application.Dashboard;

public record DashboardDto(MomentumDto Momentum, IReadOnlyList<AttentionDto> Attention, IReadOnlyList<ProjectLaneDto> Projects);
