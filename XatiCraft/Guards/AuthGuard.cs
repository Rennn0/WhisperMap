using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace XatiCraft.Guards;

/// <summary>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public abstract class AuthGuard : Attribute, IAuthorizationFilter
{
    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    public virtual void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!TryGetSessionData(context.HttpContext, out SessionData? protectedSession) || protectedSession is null)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
            return;
        }

        if (!Validate(protectedSession, context.HttpContext.RequestServices,
                key => GetHeaderValue(context.HttpContext, key)))
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
    }

    private static string? GetHeaderValue(HttpContext context, string key)
    {
        return context.Request.Headers.TryGetValue(key, out StringValues keyValues)
            ? keyValues.ToString().Split(',')[0].Trim()
            : null;
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="sessionData"></param>
    /// <returns></returns>
    protected virtual bool TryGetSessionData(HttpContext context, out SessionData? sessionData)
    {
        sessionData = null;
        if (!context.Request.Cookies.TryGetValue("session", out string? protectedSession))
            return false;
        try
        {
            IDataProtectionProvider protectionProvider =
                context.RequestServices.GetRequiredService<IDataProtectionProvider>();
            IDataProtector protector = protectionProvider.CreateProtector(GetProtectionPurpose());
            string json = protector.Unprotect(protectedSession);
            sessionData = JsonSerializer.Deserialize<SessionData>(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public static string GetProtectionPurpose()
    {
        StringBuilder builder = new StringBuilder();
        AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
        builder.Append(assemblyName.Name);
        builder.Append('_');
        builder.Append(assemblyName.Version);
        return builder.ToString();
    }

    /// <summary>
    /// </summary>
    /// <param name="sessionData"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="fromHeader"></param>
    /// <returns></returns>
    protected abstract bool Validate(SessionData sessionData, IServiceProvider serviceProvider,
        Func<string, string?> fromHeader);
}