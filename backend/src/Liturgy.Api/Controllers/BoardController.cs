using Liturgy.Application.Board;
using Liturgy.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Liturgy.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/board")]
public class BoardController : ControllerBase
{
    private readonly IMediator _mediator;

    public BoardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{projectId:guid}")]
    public async Task<ActionResult<BoardDto>> Get(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBoardQuery(projectId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("cards")]
    public async Task<ActionResult<CardDto>> CreateCard([FromBody] CreateCardRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateCardCommand(request.ProjectId, request.Title, request.Description, request.AssigneeId),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("cards/{cardId:guid}/move")]
    public async Task<ActionResult<CardDto>> MoveCard(
        Guid cardId,
        [FromBody] MoveCardRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MoveCardCommand(cardId, request.Column), cancellationToken);
        return Ok(result);
    }

    [HttpPost("cards/{cardId:guid}/assign")]
    public async Task<ActionResult<CardDto>> AssignCard(
        Guid cardId,
        [FromBody] AssignCardRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new AssignCardCommand(cardId, request.AssigneeId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("cards/{cardId:guid}/point")]
    public async Task<ActionResult<CardDto>> PointCard(
        Guid cardId,
        [FromBody] PointCardRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PointCardCommand(cardId, request.Points), cancellationToken);
        return Ok(result);
    }

    [HttpPost("cards/{cardId:guid}/cancel")]
    public async Task<ActionResult<CardDto>> CancelCard(Guid cardId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SetCardStatusCommand(cardId, CardStatus.Cancelled), cancellationToken);
        return Ok(result);
    }

    [HttpPost("cards/{cardId:guid}/close")]
    public async Task<ActionResult<CardDto>> CloseCard(Guid cardId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SetCardStatusCommand(cardId, CardStatus.Closed), cancellationToken);
        return Ok(result);
    }

    [HttpPost("cards/{cardId:guid}/reopen")]
    public async Task<ActionResult<CardDto>> ReopenCard(Guid cardId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SetCardStatusCommand(cardId, CardStatus.Open), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("cards/{cardId:guid}")]
    public async Task<IActionResult> DeleteCard(Guid cardId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCardCommand(cardId), cancellationToken);
        return NoContent();
    }
}
