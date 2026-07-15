using Liturgy.Application.Abstractions;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Invitations;

public class ListInvitationsQueryHandler : IRequestHandler<ListInvitationsQuery, IReadOnlyList<InvitationDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ListInvitationsQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<InvitationDto>> Handle(ListInvitationsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Array.Empty<InvitationDto>();
        }

        var workspaceId = await _db.Memberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => (Guid?)m.WorkspaceId)
            .FirstOrDefaultAsync(cancellationToken);

        if (workspaceId is null)
        {
            return Array.Empty<InvitationDto>();
        }

        var invitations = await _db.Invitations
            .AsNoTracking()
            .Where(i => i.WorkspaceId == workspaceId.Value && i.Status == InvitationStatus.Pending)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

        return invitations.Select(InvitationMapper.ToDto).ToList();
    }
}
