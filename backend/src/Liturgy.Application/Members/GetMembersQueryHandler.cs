using Liturgy.Application.Abstractions;
using Liturgy.Application.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Members;

/// <summary>
/// Returns the caller's workspace members — the union across every workspace they
/// belong to, deduplicated by user. Id is the user id, matching <see cref="Domain.Card.AssigneeId"/>,
/// so the client can use this list directly to populate an assignee picker.
/// </summary>
public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, IReadOnlyList<MemberDto>>
{
    private readonly IAppDbContext _db;
    private readonly IWorkspaceAccess _workspaceAccess;

    public GetMembersQueryHandler(IAppDbContext db, IWorkspaceAccess workspaceAccess)
    {
        _db = db;
        _workspaceAccess = workspaceAccess;
    }

    public async Task<IReadOnlyList<MemberDto>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var workspaceIds = await _workspaceAccess.MyWorkspaceIdsAsync(cancellationToken);

        var memberships = await _db.Memberships
            .AsNoTracking()
            .Where(m => workspaceIds.Contains(m.WorkspaceId))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        var userIds = memberships.Select(m => m.UserId).Distinct().ToList();
        var users = await _db.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        return memberships
            .GroupBy(m => m.UserId)
            .Select(g => g.First())
            .Where(m => users.ContainsKey(m.UserId))
            .Select(membership =>
            {
                var user = users[membership.UserId];
                return new MemberDto(
                    user.Id,
                    $"{user.FirstName} {user.LastName}".Trim(),
                    Initials.From(user.FirstName, user.LastName),
                    membership.Role);
            })
            .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
