using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using XatiCraft.ApiContracts;

namespace XatiCraft.Handlers;

internal class GeneralExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problem = new ProblemDetails
        {
            Status = (int)ErrorCode.UnhandledException,
            Detail =  exception.Message,
        };
        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}