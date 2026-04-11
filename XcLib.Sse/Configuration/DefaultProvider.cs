using Microsoft.Extensions.Configuration;
using XcLib.Sse.Options;

namespace XcLib.Sse.Configuration;

public class DefaultProvider : ConfigurationProvider
{
    private readonly Func<Task<SseOptions>>? _optionsLoader;

    public DefaultProvider(Func<Task<SseOptions>>? optionsLoader = null) => _optionsLoader = optionsLoader;
    public override void Load() => SetValues();

    public void Reload()
    {
        Load();
        OnReload();
    }

    private void SetValues()
    {
        const string pingInterval = "pingInterval";
        SseOptions defaultOptions = _optionsLoader?.Invoke().Result ?? new SseOptions
        {
            PingInterval = 10
        };
        
        Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            [$"{nameof(SseOptions)}:{pingInterval}"] = $"{defaultOptions.PingInterval}",
            [$"{nameof(SseSignalOptions)}:{pingInterval}"] = $"{defaultOptions.PingInterval}",
            [$"{nameof(SseStreamOptions)}:{pingInterval}"] = $"{defaultOptions.PingInterval}"
        };
    }
}