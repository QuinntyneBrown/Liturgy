using Liturgy.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Liturgy.Application.Auth;

public class SignInCommandHandler : IRequestHandler<SignInCommand, AuthResult>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenIssuer _tokens;

    public SignInCommandHandler(IAppDbContext db, IPasswordHasher hasher, IJwtTokenIssuer tokens)
    {
        _db = db;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<AuthResult> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

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
