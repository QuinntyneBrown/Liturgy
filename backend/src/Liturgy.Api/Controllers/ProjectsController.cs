using Liturgy.Application.Discernment;
using Liturgy.Application.Impact;
using Liturgy.Application.Projects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Liturgy.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectSummaryDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListProjectsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectSummaryDto>> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateProjectCommand(request.Name, request.Tag), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectJourneyDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProjectQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/decision")]
    public async Task<ActionResult<DecisionDto>> GetDecision(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDecisionQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/decision")]
    public async Task<ActionResult<DecisionDto>> UpdateDecision(
        Guid id,
        [FromBody] UpdateDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDecisionCommand(id, request.ChosenPath, request.Rationale, request.PrayedOverWith);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/impact")]
    public async Task<ActionResult<ImpactDto>> GetImpact(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetImpactQuery(id), cancellationToken);
        return Ok(result);
    }
}
