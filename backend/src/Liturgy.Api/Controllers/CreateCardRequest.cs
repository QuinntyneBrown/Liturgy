namespace Liturgy.Api.Controllers;

public record CreateCardRequest(Guid ProjectId, string Title, string? Description, Guid? AssigneeId);
