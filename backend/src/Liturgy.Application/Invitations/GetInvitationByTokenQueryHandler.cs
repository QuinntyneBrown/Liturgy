using Liturgy.Application.Abstractions;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Invitations;

public class GetInvitationByTokenQueryHandler : IRequestHandler<GetInvitationByTokenQuery, InvitationPreviewDto>
{
    private readonly IAppDbContext _db;

    public GetInvitationByTokenQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<InvitationPreviewDto> Handle(GetInvitationByTokenQuery request, CancellationToken cancellationToken)
    {
        var invitation = await _db.Invitations
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Token == request.Token && i.Status == InvitationStatus.Pending, cancellationToken)
            ?? throw new InvitationNotFoundException(request.Token);

        var workspaceName = await _db.Workspaces
            .AsNoTracking()
            .Where(w => w.Id == invitation.WorkspaceId)
            .Select(w => w.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? "a workspace";

        var invitedByName = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == invitation.InvitedByUserId)
            .Select(u => (u.FirstName + " " + u.LastName).Trim())
            .FirstOrDefaultAsync(cancellationToken) ?? "A workspace lead";

        return new InvitationPreviewDto(workspaceName, invitedByName, invitation.Email);
    }
}
