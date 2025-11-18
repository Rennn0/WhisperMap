using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace XatiCraft.Guards;

/// <summary>
///     defines basic functions how to validate client requests
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public abstract class AuthGuard : Attribute, IAuthorizationFilter
{
    /// <summary>
    ///     client must have cookie
    /// </summary>
    public const string SessionCookie = "_se";

    /// <summary>
    ///     policy for rate limiting by session
    /// </summary>
    public const string SessionPolicy = "se_policy";

    /// <summary>
    ///     must header for initializing session cookie
    /// </summary>
    public const string InitHeader = "x-public-ip";

    private ILogger<AuthGuard>? _logger;

    /// <summary>
    ///     parses <see cref="SessionCookie" /> into <see cref="SessionData" /> and
    ///     gives it to abstract <see cref="Validate" /> to validate request
    /// </summary>
    /// <param name="context"></param>
    public virtual void OnAuthorization(AuthorizationFilterContext context)
    {
        _logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<AuthGuard>>();
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
    ///     reads <see cref="SessionCookie" /> cookie and tries to parse it,
    ///     true if it was ok, else false
    /// </summary>
    /// <param name="context"></param>
    /// <param name="sessionData"></param>
    /// <returns></returns>
    protected virtual bool TryGetSessionData(HttpContext context, out SessionData? sessionData)
    {
        sessionData = null;
        if (!context.Request.Cookies.TryGetValue(SessionCookie, out string? protectedSession))
        {
            _logger ??= context.RequestServices.GetRequiredService<ILogger<AuthGuard>>();
            _logger.LogWarning("no session cookie");
            return false;
        }

        try
        {
            Security security =
                context.RequestServices.GetRequiredService<IEnumerable<Security>>().First(s => s is AspDataProtector);
            string json = security.UnPack(protectedSession);
            sessionData = JsonSerializer.Deserialize<SessionData>(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     implement logic how to handle request, return true if it's ok
    /// </summary>
    /// <param name="sessionData"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="fromHeader"></param>
    /// <returns></returns>
    protected abstract bool Validate(SessionData sessionData, IServiceProvider serviceProvider,
        Func<string, string?> fromHeader);
}