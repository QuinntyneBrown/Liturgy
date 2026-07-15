using FluentValidation;
using Liturgy.Application.Auth;
using Liturgy.Application.Enforcement;
using Liturgy.Application.Invitations;
using Liturgy.Application.Loop;
using Microsoft.AspNetCore.Mvc;

namespace Liturgy.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private const string ProblemJson = "application/problem+json";

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var problem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            };
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(problem, options: null, contentType: ProblemJson);
        }
        catch (EmailAlreadyRegisteredException)
        {
            await WriteProblem(context, StatusCodes.Status409Conflict, "Email already registered.");
        }
        catch (InvalidCredentialsException)
        {
            await WriteProblem(context, StatusCodes.Status401Unauthorized, "Invalid credentials.");
        }
        catch (MovementsIncompleteException ex)
        {
            await WriteProblem(context, StatusCodes.Status409Conflict, "The 5R loop is incomplete.", ex.Message);
        }
        catch (OutOfOrderMovementException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Movement logged out of order.", ex.Message);
        }
        catch (GateLockedException ex)
        {
            await WriteProblem(context, StatusCodes.Status409Conflict, "Gate is blocked.", ex.Message);
        }
        catch (CardNotFoundException ex)
        {
            await WriteProblem(context, StatusCodes.Status404NotFound, "Card not found.", ex.Message);
        }
        catch (ProjectNotFoundException ex)
        {
            await WriteProblem(context, StatusCodes.Status404NotFound, "Project not found.", ex.Message);
        }
        catch (RequirementNotFoundException ex)
        {
            await WriteProblem(context, StatusCodes.Status404NotFound, "Requirement not found.", ex.Message);
        }
        catch (InvitationNotFoundException ex)
        {
            await WriteProblem(context, StatusCodes.Status404NotFound, "Invitation not found.", ex.Message);
        }
        catch (InvitationNotPendingException ex)
        {
            await WriteProblem(context, StatusCodes.Status409Conflict, "Invitation is no longer pending.", ex.Message);
        }
        catch (NotWorkspaceLeadException ex)
        {
            await WriteProblem(context, StatusCodes.Status403Forbidden, "Only a workspace Lead can do that.", ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblem(context, StatusCodes.Status500InternalServerError, "Internal server error.");
        }
    }

    private static Task WriteProblem(HttpContext context, int status, string title, string? detail = null)
    {
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        }, options: null, contentType: ProblemJson);
    }
}
