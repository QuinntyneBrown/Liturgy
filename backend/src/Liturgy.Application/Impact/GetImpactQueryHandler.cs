using Liturgy.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Impact;

public class GetImpactQueryHandler : IRequestHandler<GetImpactQuery, ImpactDto>
{
    private const string Headline = "Impact is friendship, compounded by time.";

    private readonly IAppDbContext _db;
    private readonly IWorkspaceAccess _workspaceAccess;

    public GetImpactQueryHandler(IAppDbContext db, IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<ImpactDto> Handle(GetImpactQuery request, CancellationToken cancellationToken)
    {
        await _workspaceAccess.EnsureProjectVisibleAsync(request.ProjectId, cancellationToken);

        var metrics = await _db.ImpactMetrics
            .AsNoTracking()
            .Where(m => m.ProjectId == request.ProjectId)
            .OrderBy(m => m.Order)
            .ToListAsync(cancellationToken);

        var stories = await _db.Stories
            .AsNoTracking()
            .Where(s => s.ProjectId == request.ProjectId)
            .OrderBy(s => s.Week)
            .ThenBy(s => s.Order)
            .ToListAsync(cancellationToken);

        var gratitude = await _db.Gratitudes
            .AsNoTracking()
            .Where(g => g.ProjectId == request.ProjectId)
            .OrderBy(g => g.Order)
            .ToListAsync(cancellationToken);

        return new ImpactDto(
            Headline,
            metrics.Select(ImpactMetricDto.From).ToList(),
            stories.Select(StoryDto.From).ToList(),
            gratitude.Select(GratitudeDto.From).ToList());
    }
}
