using Liturgy.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Projects;

public class ListProjectsQueryHandler : IRequestHandler<ListProjectsQuery, IReadOnlyList<ProjectSummaryDto>>
{
    private readonly IAppDbContext _db;

    public ListProjectsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProjectSummaryDto>> Handle(ListProjectsQuery request, CancellationToken cancellationToken)
    {
        return await _db.Projects
            .AsNoTracking()
            .OrderBy(p => p.CreatedAt)
            .Select(p => new ProjectSummaryDto(p.Id, p.Name, p.Tag, p.CurrentPhase))
            .ToListAsync(cancellationToken);
    }
}
