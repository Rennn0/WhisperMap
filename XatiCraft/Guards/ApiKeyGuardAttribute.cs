using Microsoft.Extensions.Options;
using XatiCraft.Settings;

namespace XatiCraft.Guards;

public class ApiKeyGuardAttribute : AuthGuard
{
    protected override bool Validate(SessionData sessionData, IServiceProvider serviceProvider,
        Func<string, string?> fromHeader)
    {
        IOptionsSnapshot<ApiKeySettings> settings =
            serviceProvider.GetRequiredService<IOptionsSnapshot<ApiKeySettings>>();
        List<string> allowedList = settings.Value.AllowedKeys;
        string? apiKey = fromHeader("x-api-key");
        return !string.IsNullOrEmpty(apiKey) && allowedList.Contains(apiKey);
    }
}