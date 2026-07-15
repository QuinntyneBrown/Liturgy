using Liturgy.Application.Abstractions;
using Liturgy.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenIssuer _tokens;
    private readonly IClock _clock;

    public RegisterCommandHandler(
        IAppDbContext db,
        IPasswordHasher hasher,
        IJwtTokenIssuer tokens,
        IClock clock)
    {
        _db = db;
        _hasher = hasher;
        _tokens = tokens;
        _clock = clock;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existing = await _db.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (existing)
        {
            throw new EmailAlreadyRegisteredException(normalizedEmail);
        }

        var now = _clock.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PasswordHash = _hasher.Hash(request.Password),
            Role = "Member",
            CreatedAt = now
        };
        _db.Users.Add(user);

        // Every registrant gets their own (empty) workspace, so they never see another
        // workspace's projects until they're invited into one.
        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = $"{user.FirstName}'s Workspace",
            Slug = "ws-" + Guid.NewGuid().ToString("N")[..12],
            CreatedAt = now
        };
        _db.Workspaces.Add(workspace);

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            UserId = user.Id,
            Role = "Lead",
            Initials = Initials.From(user.FirstName, user.LastName),
            CreatedAt = now
        };
        _db.Memberships.Add(membership);

        await _db.SaveChangesAsync(cancellationToken);

        var accessToken = _tokens.Issue(user);
        return new AuthResult(
            accessToken,
            user.Id,
            user.Email,
            user.Role,
            user.FirstName,
            user.LastName,
            Initials.From(user.FirstName, user.LastName));
    }
}
