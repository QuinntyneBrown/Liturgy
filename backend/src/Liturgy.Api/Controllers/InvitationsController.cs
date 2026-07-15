using Liturgy.Application.Invitations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Liturgy.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/invitations")]
public class InvitationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvitationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvitationDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListInvitationsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<InvitationDto>> Create(
        [FromBody] CreateInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateInvitationCommand(request.Email, request.Role), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<InvitationPreviewDto>> GetByToken(string token, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetInvitationByTokenQuery(token), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{token}/accept")]
    public async Task<IActionResult> Accept(string token, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AcceptInvitationCommand(token), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RevokeInvitationCommand(id), cancellationToken);
        return NoContent();
    }
}
