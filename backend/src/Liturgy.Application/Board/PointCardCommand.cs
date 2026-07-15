using MediatR;

namespace Liturgy.Application.Board;

/// <summary>Sets (or clears, when null) a card's story-point estimate.</summary>
public record PointCardCommand(Guid CardId, int? Points) : IRequest<CardDto>;
