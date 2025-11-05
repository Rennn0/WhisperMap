using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace XatiCraft.Guards;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class IpSessionGuardAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        ILogger logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<IpSessionGuardAttribute>>();
        bool hasSession = context.HttpContext.Request.Cookies.TryGetValue("session", out string? sessionId) &&
                          !string.IsNullOrEmpty(sessionId);

        if (hasSession) return;

        logger.LogWarning("blocked request");
        context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
    }
}