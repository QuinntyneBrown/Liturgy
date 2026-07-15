using FluentValidation;
using FluentValidation.Results;
using Liturgy.Application.Abstractions;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Invitations;

/// <summary>
/// A workspace Lead invites someone by email. Idempotent: a second invite to the same
/// still-pending email returns the existing one rather than creating a duplicate.
/// </summary>
public class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, InvitationDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public CreateInvitationCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<InvitationDto> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not { } userId)
        {
            throw new UnauthorizedAccessException();
        }

        var membership = await _db.Memberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.Email), "You must belong to a workspace before inviting others.")
            });

        if (!string.Equals(membership.Role, "Lead", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotWorkspaceLeadException();
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var role = string.IsNullOrWhiteSpace(request.Role) ? "Member" : request.Role.Trim();

        var alreadyMember = await _db.Memberships
            .AsNoTracking()
            .Join(_db.Users, m => m.UserId, u => u.Id, (m, u) => new { m.WorkspaceId, u.Email })
            .AnyAsync(x => x.WorkspaceId == membership.WorkspaceId && x.Email == email, cancellationToken);
        if (alreadyMember)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.Email), "That person is already a member of this workspace.")
            });
        }

        var existing = await _db.Invitations.FirstOrDefaultAsync(
            i => i.WorkspaceId == membership.WorkspaceId && i.Email == email && i.Status == InvitationStatus.Pending,
            cancellationToken);
        if (existing is not null)
        {
            return InvitationMapper.ToDto(existing);
        }

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            WorkspaceId = membership.WorkspaceId,
            Email = email,
            Token = InvitationToken.New(),
            Role = role,
            Status = InvitationStatus.Pending,
            InvitedByUserId = userId,
            CreatedAt = _clock.UtcNow
        };
        _db.Invitations.Add(invitation);
        await _db.SaveChangesAsync(cancellationToken);

        return InvitationMapper.ToDto(invitation);
    }
}
