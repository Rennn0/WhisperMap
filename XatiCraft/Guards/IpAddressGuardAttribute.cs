using Microsoft.Extensions.Options;
using XatiCraft.Settings;

namespace XatiCraft.Guards;

/// <summary>
/// </summary>
public class IpAddressGuardAttribute : AuthGuard
{
    /// <summary>
    /// </summary>
    /// <param name="sessionData"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="fromHeader"></param>
    /// <returns></returns>
    protected override bool Validate(SessionData sessionData, IServiceProvider serviceProvider,
        Func<string, string?> fromHeader)
    {
        IOptionsSnapshot<IpRestrictionSettings> ipSettings =
            serviceProvider.GetRequiredService<IOptionsSnapshot<IpRestrictionSettings>>();
        ILogger<IpAddressGuardAttribute>
            logger = serviceProvider.GetRequiredService<ILogger<IpAddressGuardAttribute>>();

        List<string> allowedList = ipSettings.Value.AllowedIpAddresses;

        if (sessionData is not { Ip.Length: > 0 } || !allowedList.Contains(sessionData.Ip))
        {
            logger.LogWarning("blocked ip {ip}", sessionData?.Ip);
            return false;
        }

        logger.LogInformation("client ip {ip}", sessionData.Ip);
        return true;
    }
}