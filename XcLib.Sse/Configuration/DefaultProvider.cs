using Microsoft.Extensions.Configuration;
using XcLib.Sse.Options;

namespace XcLib.Sse.Configuration;

public class DefaultProvider : ConfigurationProvider
{
    public override void Load()
    {
        const string pingInterval = "pingInterval";
        Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            [$"{nameof(SseOptions)}:{pingInterval}"] = "7",
            [$"{nameof(SseSignalOptions)}:{pingInterval}"] = "3",
            [$"{nameof(SseStreamOptions)}:{pingInterval}"] = "10"
        };
    }
}