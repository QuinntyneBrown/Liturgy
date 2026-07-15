using MediatR;

namespace Liturgy.Application.Board;

/// <summary>Permanently deletes a card and its 5R movements.</summary>
public record DeleteCardCommand(Guid CardId) : IRequest;
