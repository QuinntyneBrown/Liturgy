namespace Liturgy.Domain;

public class RMovement
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public RKind Kind { get; set; }
    public int Order { get; set; }
    public MovementState State { get; set; } = MovementState.Locked;

    // Content captured per R (only the field for the given Kind is populated).
    public string? Ask { get; set; }
    public string? Received { get; set; }
    public string? Synthesis { get; set; }
    public string? ArtifactUrl { get; set; }
    public string? WhatChanged { get; set; }
    public string? Thanksgiving { get; set; }

    public DateTimeOffset? LoggedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
