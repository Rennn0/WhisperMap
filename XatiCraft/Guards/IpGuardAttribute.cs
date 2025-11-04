using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using XatiCraft.Settings;

namespace XatiCraft.Guards;

public class IpGuardAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        IServiceProvider serviceProvider = context.HttpContext.RequestServices;
        IOptionsSnapshot<IpRestrictionSettings> ipSettings =
            serviceProvider.GetRequiredService<IOptionsSnapshot<IpRestrictionSettings>>();
        ILogger<IpGuardAttribute> logger =
            serviceProvider.GetRequiredService<ILogger<IpGuardAttribute>>();

        List<string> allowedList = ipSettings.Value.AllowedIpAddresses;

        IPAddress? remoteIp = context.HttpContext.Connection.RemoteIpAddress;
        if (remoteIp is null) return Forbidden();

        string ip = remoteIp.ToString();
        logger.LogInformation(ip);

        return !allowedList.Contains(ip) ? Forbidden() : Task.CompletedTask;

        Task Forbidden()
        {
            context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
            return Task.CompletedTask;
        }
    }
}