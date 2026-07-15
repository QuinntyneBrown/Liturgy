using FluentValidation;
using FluentValidation.Results;
using Liturgy.Application.Abstractions;
using Liturgy.Application.Auth;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Invitations;

/// <summary>
/// Adds the current user to the inviting workspace. The invite's email must match the
/// caller's, so a leaked token can't be redeemed by someone else. Idempotent if already a member.
/// </summary>
public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public AcceptInvitationCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            throw new UnauthorizedAccessException();
        }

        var invitation = await _db.Invitations.FirstOrDefaultAsync(i => i.Token == request.Token, cancellationToken)
            ?? throw new InvitationNotFoundException(request.Token);

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvitationNotPendingException();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new UnauthorizedAccessException();

        if (!string.Equals(user.Email, invitation.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.Token), "This invitation was issued to a different email address.")
            });
        }

        var now = _clock.UtcNow;

        var alreadyMember = await _db.Memberships
            .AnyAsync(m => m.WorkspaceId == invitation.WorkspaceId && m.UserId == userId, cancellationToken);
        if (!alreadyMember)
        {
            _db.Memberships.Add(new Membership
            {
                Id = Guid.NewGuid(),
                WorkspaceId = invitation.WorkspaceId,
                UserId = userId,
                Role = invitation.Role,
                Initials = Initials.From(user.FirstName, user.LastName),
                CreatedAt = now
            });
        }

        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedAt = now;

        await _db.SaveChangesAsync(cancellationToken);
    }
}
