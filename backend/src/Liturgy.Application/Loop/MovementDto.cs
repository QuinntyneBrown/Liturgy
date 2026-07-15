using Liturgy.Domain;

namespace Liturgy.Application.Loop;

public record MovementDto(
    Guid Id,
    RKind Kind,
    int Order,
    MovementState State,
    string? Ask,
    string? Received,
    string? Synthesis,
    string? ArtifactUrl,
    string? WhatChanged,
    string? Thanksgiving,
    DateTimeOffset? LoggedAt)
{
    public static MovementDto From(RMovement m) => new(
        m.Id,
        m.Kind,
        m.Order,
        m.State,
        m.Ask,
        m.Received,
        m.Synthesis,
        m.ArtifactUrl,
        m.WhatChanged,
        m.Thanksgiving,
        m.LoggedAt);
}
