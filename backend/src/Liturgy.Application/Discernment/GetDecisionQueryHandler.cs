using Liturgy.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Discernment;

public class GetDecisionQueryHandler : IRequestHandler<GetDecisionQuery, DecisionDto>
{
    private readonly IAppDbContext _db;
    private readonly IWorkspaceAccess _workspaceAccess;

    public GetDecisionQueryHandler(IAppDbContext db, IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<DecisionDto> Handle(GetDecisionQuery request, CancellationToken cancellationToken)
    {
        await _workspaceAccess.EnsureProjectVisibleAsync(request.ProjectId, cancellationToken);

        var decision = await _db.Decisions
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ProjectId == request.ProjectId, cancellationToken);

        return decision is null ? DecisionDto.Empty : DecisionDto.From(decision);
    }
}
