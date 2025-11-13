using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using XatiCraft.Settings;

namespace XatiCraft.Guards;

/// <summary>
/// </summary>
public class ApiKeyGuardAttribute : AuthGuard
{
    /// <inheritdoc />
    public override void OnAuthorization(AuthorizationFilterContext context)
    {
        IServiceProvider serviceProvider = context.HttpContext.RequestServices;
        IOptionsSnapshot<ApiKeySettings> settings =
            serviceProvider.GetRequiredService<IOptionsSnapshot<ApiKeySettings>>();
        List<string> allowedList = settings.Value.AllowedKeys;
        string? apiKey = context.HttpContext.Request.Headers["x-api-key"];

        if (!string.IsNullOrEmpty(apiKey) && allowedList.Contains(apiKey))
            return;

        context.Result = new UnauthorizedResult();
    }

    /// <summary>
    /// </summary>
    /// <param name="sessionData"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="fromHeader"></param>
    /// <returns></returns>
    protected override bool Validate(SessionData sessionData, IServiceProvider serviceProvider,
        Func<string, string?> fromHeader)
    {
        throw new NotImplementedException();
    }
}