using Microsoft.Extensions.Configuration;
using XcLib.Sse.Options;

namespace XcLib.Sse.Configuration;

public class DefaultSource : IConfigurationSource
{
    private readonly IConfigurationTrigger? _trigger;
    private readonly Func<Task<SseOptions>>? _optionsLoader;

    public DefaultSource()
    {
    }

    public DefaultSource(IConfigurationTrigger trigger) => _trigger = trigger;

    public DefaultSource(SseOptionsReloadTrigger trigger, Func<Task<SseOptions>> optionsLoader)
    {
        _trigger = trigger;
        _optionsLoader = optionsLoader;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        DefaultProvider provider = new DefaultProvider(_optionsLoader);
        _trigger?.Register(provider.Reload);

        return provider;
    }
}