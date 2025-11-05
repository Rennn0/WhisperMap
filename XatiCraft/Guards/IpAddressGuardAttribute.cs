using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using XatiCraft.Settings;

namespace XatiCraft.Guards;

public class IpAddressGuardAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        IServiceProvider serviceProvider = context.HttpContext.RequestServices;

        IOptionsSnapshot<IpRestrictionSettings> ipSettings =
            serviceProvider.GetRequiredService<IOptionsSnapshot<IpRestrictionSettings>>();
        ILogger<IpAddressGuardAttribute>
            logger = serviceProvider.GetRequiredService<ILogger<IpAddressGuardAttribute>>();

        List<string> allowedList = ipSettings.Value.AllowedIpAddresses;

        string? ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
        logger.LogInformation("remote ip: {ip}", ip);
        if (!context.HttpContext.Request.Headers.TryGetValue("x-forwarded-for", out StringValues forwardHeader))
            return !allowedList.Contains(ip ?? string.Empty) ? Forbidden() : Task.CompletedTask;

        ip = forwardHeader.ToString().Split(',')[0].Trim();
        logger.LogInformation("forwarded ip: {ip}", ip);

        return !allowedList.Contains(ip) ? Forbidden() : Task.CompletedTask;

        Task Forbidden()
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            return Task.CompletedTask;
        }
    }
}