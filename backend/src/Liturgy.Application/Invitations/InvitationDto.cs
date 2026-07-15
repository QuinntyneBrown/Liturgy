using Liturgy.Domain;

namespace Liturgy.Application.Invitations;

/// <summary>
/// A workspace invitation as seen by a Lead managing their account. Carries the opaque
/// <see cref="Token"/> and a relative <see cref="InvitePath"/> the client turns into a
/// shareable link (<c>{origin}/sign-up?token={token}</c>).
/// </summary>
public record InvitationDto(
    Guid Id,
    string Email,
    string Role,
    InvitationStatus Status,
    string Token,
    string InvitePath,
    DateTimeOffset CreatedAt);
