using ASA.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ASA.Api;

internal class GeneralExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public GeneralExceptionHandler(IProblemDetailsService problemDetailsService) =>
        _problemDetailsService = problemDetailsService;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails = exception switch
        {
            ConcurrencyException e => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Version conflict",
                Detail = "This request was changed by someone else. Reload it and try again.",
                Extensions = { ["currentVersion"] = e.ActualVersion, ["msg"] = e.Message }
            },
            DomainException e => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Domain Error",
                Detail = e.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Detail = exception.Message
            }
        };

        if (problemDetails.Status.HasValue)
            httpContext.Response.StatusCode = problemDetails.Status.Value;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails, Exception = exception
        });
    }
}