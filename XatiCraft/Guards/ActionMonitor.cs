using Microsoft.AspNetCore.Mvc.Filters;

namespace XatiCraft.Guards;

/// <summary>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ActionMonitor : Attribute, IAsyncActionFilter
{
    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ILogger<ActionMonitor> logger =
            context.HttpContext.RequestServices.GetRequiredService<ILogger<ActionMonitor>>();
        HttpRequest request = context.HttpContext.Request;
        logger.LogInformation("Request: {Method} {Path} Query: {QueryString}",
            request.Method,
            request.Path.ToString(),
            request.QueryString.ToString()
        );

        ActionExecutedContext executed = await next();

        HttpResponse response = executed.HttpContext.Response;
        logger.LogInformation("Response: {StatusCode}",
            response.StatusCode
        );
    }
}