using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using XatiCraft.Guards;

namespace XatiCraft.Controllers;

/// <inheritdoc />
public class ApplicationController : ControllerBase
{
    /// <summary>
    /// </summary>
    protected AppMetrics AppMetrics => HttpContext.RequestServices.GetRequiredService<AppMetrics>();

    private readonly CookieOptions _defaultCookieOptions = new CookieOptions
    {
        HttpOnly = true, Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.Now.AddDays(1)
    };

    /// <summary>
    ///     User Id Cookie
    /// </summary>
    protected string? UserIdC => HttpContext.Request.Cookies[AuthGuard.UserIdCookie];

    /// <summary>
    ///     Append cookie to client
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    protected void AppendC(string key, string value, CookieOptions? options = null) =>
        HttpContext.Response.Cookies.Append(key, value, options ?? _defaultCookieOptions);

    /// <summary>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="options"></param>
    protected void DeleteC(string key, CookieOptions? options = null) =>
        HttpContext.Response.Cookies.Delete(key, options ?? _defaultCookieOptions);

    /// <summary>
    /// </summary>
    /// <param name="func"></param>
    /// <param name="caller"></param>
    protected async Task RunWithMeasurementAsync(Func<Task> func, [CallerMemberName] string caller = "")
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        KeyValuePair<string, object?> funcTag = new KeyValuePair<string, object?>("func", func.Method.Name);
        KeyValuePair<string, object?> callerTag = new KeyValuePair<string, object?>("caller", caller);
        try
        {
            await func();
        }
        catch (Exception e)
        {
            AppMetrics.UnhandledExceptionMetric.Add(1,
                new KeyValuePair<string, object?>(e.GetType().Name, caller));
        }
        finally
        {
            stopwatch.Stop();
            AppMetrics.FuncExecutionMetric.Record((float)stopwatch.Elapsed.TotalMilliseconds, funcTag, callerTag);
        }
    }
}