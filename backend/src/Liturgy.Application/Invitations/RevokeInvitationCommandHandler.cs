using Liturgy.Application.Abstractions;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Invitations;

public class RevokeInvitationCommandHandler : IRequestHandler<RevokeInvitationCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public RevokeInvitationCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(RevokeInvitationCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            throw new UnauthorizedAccessException();
        }

        var membership = await _db.Memberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (membership is null || !string.Equals(membership.Role, "Lead", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotWorkspaceLeadException();
        }

        var invitation = await _db.Invitations.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        // A cross-workspace id is indistinguishable from a missing one, by design.
        if (invitation is null || invitation.WorkspaceId != membership.WorkspaceId)
        {
            throw new InvitationNotFoundException(request.Id.ToString());
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvitationNotPendingException();
        }

        invitation.Status = InvitationStatus.Revoked;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
