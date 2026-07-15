using Liturgy.Domain;
using MediatR;

namespace Liturgy.Application.Board;

/// <summary>
/// Sets a card's lifecycle <see cref="CardStatus"/>. Closing or cancelling lifts the card off
/// the active board; reopening (Open) restores it. Independent of the 5R Done gate — allowed
/// from any board column or loop state.
/// </summary>
public record SetCardStatusCommand(Guid CardId, CardStatus Status) : IRequest<CardDto>;
