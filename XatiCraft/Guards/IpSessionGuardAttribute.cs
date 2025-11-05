using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace XatiCraft.Guards;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class IpSessionGuardAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        bool hasIp = context.HttpContext.Request.Cookies.TryGetValue("client_ip", out string? clientIp) &&
                     !string.IsNullOrEmpty(clientIp);
        bool hasSession = context.HttpContext.Request.Cookies.TryGetValue("session_id", out string? sessionId) &&
                          !string.IsNullOrEmpty(sessionId);

        if (!hasIp || !hasSession)
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
    }
}