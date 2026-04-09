using Microsoft.Extensions.Configuration;
using XcLib.Sse.Configuration;

namespace XcLib.Sse;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder ConfigureSseDefaults(this IConfigurationBuilder manager)
        => manager.Add(new DefaultSource());
}