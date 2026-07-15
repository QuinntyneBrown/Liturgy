using Liturgy.Application.Board;
using Liturgy.Application.Loop;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Liturgy.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/loop")]
public class LoopController : ControllerBase
{
    private readonly IMediator _mediator;

    public LoopController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("cards/{cardId:guid}")]
    public async Task<ActionResult<CardLoopDto>> Get(Guid cardId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCardLoopQuery(cardId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("cards/{cardId:guid}/movements")]
    public async Task<ActionResult<CardLoopDto>> LogMovement(
        Guid cardId,
        [FromBody] LogMovementRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LogMovementCommand(
            cardId,
            request.Kind,
            request.Ask,
            request.Received,
            request.Synthesis,
            request.ArtifactUrl,
            request.WhatChanged,
            request.Thanksgiving);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("cards/{cardId:guid}/done")]
    public async Task<ActionResult<CardDto>> MarkDone(Guid cardId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MarkCardDoneCommand(cardId), cancellationToken);
        return Ok(result);
    }
}
