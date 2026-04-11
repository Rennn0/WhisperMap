using Microsoft.Extensions.Configuration;
using XcLib.Sse.Configuration;
using XcLib.Sse.Options;

namespace XcLib.Sse;

public static class ConfigurationExtensions
{
    public static readonly SseOptionsReloadTrigger Trigger = new SseOptionsReloadTrigger();

    public static IConfigurationBuilder ConfigureSseDefaults(this IConfigurationBuilder manager)
        => manager.Add(new DefaultSource(Trigger));

    public static IConfigurationBuilder ConfigureSseDefaults(this IConfigurationBuilder manager,
        Func<Task<SseOptions>> optionsLoader)
        => manager.Add(new DefaultSource(Trigger, optionsLoader));
}