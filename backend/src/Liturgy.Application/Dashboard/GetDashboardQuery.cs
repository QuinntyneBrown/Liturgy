using MediatR;

namespace Liturgy.Application.Dashboard;

public record GetDashboardQuery : IRequest<DashboardDto>;
