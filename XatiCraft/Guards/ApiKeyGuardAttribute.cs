using Microsoft.Extensions.Options;
using XatiCraft.Settings;

namespace XatiCraft.Guards;

/// <summary>
/// </summary>
public class ApiKeyGuardAttribute : AuthGuard
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
        IOptionsSnapshot<ApiKeySettings> settings =
            serviceProvider.GetRequiredService<IOptionsSnapshot<ApiKeySettings>>();
        List<string> allowedList = settings.Value.AllowedKeys;
        string? apiKey = fromHeader("x-api-key");
        return !string.IsNullOrEmpty(apiKey) && allowedList.Contains(apiKey);
    }
}