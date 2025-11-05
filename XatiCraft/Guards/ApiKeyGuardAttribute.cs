using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using XatiCraft.Settings;

namespace XatiCraft.Guards;

public class ApiKeyGuardAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        IServiceProvider serviceProvider = context.HttpContext.RequestServices;

        IOptionsSnapshot<ApiKeySettings> settings =
            serviceProvider.GetRequiredService<IOptionsSnapshot<ApiKeySettings>>();
        ILogger<ApiKeyGuardAttribute> logger = serviceProvider.GetRequiredService<ILogger<ApiKeyGuardAttribute>>();

        List<string> allowedList = settings.Value.AllowedKeys;
        string apiKey = "";
        if (context.HttpContext.Request.Headers.TryGetValue("x-api-key", out StringValues apiKeyValues))
            apiKey = apiKeyValues.ToString().Split(',')[0].Trim();

        logger.LogInformation("client api key: {ApiKey}", apiKey);

        return !allowedList.Contains(apiKey) ? Forbidden() : Task.CompletedTask;

        Task Forbidden()
        {
            context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
            return Task.CompletedTask;
        }
    }
}