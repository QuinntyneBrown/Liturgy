using Liturgy.Application.Gates;
using Liturgy.Application.Projects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Liturgy.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/gates")]
public class GatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public GatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("requirements/{requirementId:guid}/toggle")]
    public async Task<ActionResult<GateDto>> ToggleRequirement(
        Guid requirementId,
        [FromBody] ToggleRequirementRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ToggleRequirementCommand(requirementId, request.Done), cancellationToken);
        return Ok(result);
    }
}
