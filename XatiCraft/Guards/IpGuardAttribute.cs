using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using XatiCraft.Settings;

namespace XatiCraft.Guards;

public class IpGuardAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        IServiceProvider serviceProvider = context.HttpContext.RequestServices;

        IOptionsSnapshot<IpRestrictionSettings> ipSettings =
            serviceProvider.GetRequiredService<IOptionsSnapshot<IpRestrictionSettings>>();
        ILogger<IpGuardAttribute> logger = serviceProvider.GetRequiredService<ILogger<IpGuardAttribute>>();

        List<string> allowedList = ipSettings.Value.AllowedIpAddresses;

        string? ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();

        if (context.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardHeader))
            ip = forwardHeader.ToString().Split(',')[0].Trim();

        logger.LogInformation("Client IP: {Ip}", ip);

        return !allowedList.Contains(ip ?? string.Empty) ? Forbidden() : Task.CompletedTask;

        Task Forbidden()
        {
            context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
            return Task.CompletedTask;
        }
    }
}