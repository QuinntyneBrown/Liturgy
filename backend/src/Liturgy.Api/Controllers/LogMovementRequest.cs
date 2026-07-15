using Liturgy.Domain;

namespace Liturgy.Api.Controllers;

public record LogMovementRequest(
    RKind Kind,
    string? Ask,
    string? Received,
    string? Synthesis,
    string? ArtifactUrl,
    string? WhatChanged,
    string? Thanksgiving);
