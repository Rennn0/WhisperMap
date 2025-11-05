using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using XatiCraft.Settings;

namespace XatiCraft.Guards;

public class IpAddressGuardAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Cookies.TryGetValue("session", out string? protectedSession))
            return Forbidden();

        IServiceProvider serviceProvider = context.HttpContext.RequestServices;
        IOptionsSnapshot<IpRestrictionSettings> ipSettings =
            serviceProvider.GetRequiredService<IOptionsSnapshot<IpRestrictionSettings>>();
        ILogger<IpAddressGuardAttribute>
            logger = serviceProvider.GetRequiredService<ILogger<IpAddressGuardAttribute>>();
        IDataProtectionProvider dataProtectionProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>();
        IDataProtector dataProtector = dataProtectionProvider.CreateProtector("XatiCraft.SessionCookie");
        SessionData? sessionData = null;
        try
        {
            string json = dataProtector.Unprotect(protectedSession);
            sessionData = JsonSerializer.Deserialize<SessionData>(json);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while deserializing session data");
            return Forbidden();
        }

        List<string> allowedList = ipSettings.Value.AllowedIpAddresses;

        if (sessionData is not { Ip.Length: > 0 } || !allowedList.Contains(sessionData.Ip))
        {
            logger.LogWarning("blocked ip: {ip}", sessionData?.Ip);
            return Forbidden();
        }

        logger.LogInformation("client ip: {ip}", sessionData.Ip);
        return Task.CompletedTask;

        Task Forbidden()
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            return Task.CompletedTask;
        }
    }
}