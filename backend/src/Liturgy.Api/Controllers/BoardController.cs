using Liturgy.Application.Board;
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
        var result = await _mediator.Send(new CreateCardCommand(request.ProjectId, request.Title, request.AssigneeId), cancellationToken);
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
}
