using Liturgy.Application.Members;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Liturgy.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/members")]
public class MembersController : ControllerBase
{
    private readonly IMediator _mediator;

    public MembersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MemberDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMembersQuery(), cancellationToken);
        return Ok(result);
    }
}
