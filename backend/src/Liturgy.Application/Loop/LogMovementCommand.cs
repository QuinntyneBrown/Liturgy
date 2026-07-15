using Liturgy.Domain;
using MediatR;

namespace Liturgy.Application.Loop;

/// <summary>Logs the card's current R movement, capturing its content and advancing the loop.</summary>
public record LogMovementCommand(
    Guid CardId,
    RKind Kind,
    string? Ask,
    string? Received,
    string? Synthesis,
    string? ArtifactUrl,
    string? WhatChanged,
    string? Thanksgiving) : IRequest<CardLoopDto>;
