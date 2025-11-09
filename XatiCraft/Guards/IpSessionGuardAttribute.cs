namespace XatiCraft.Guards;

/// <summary>
/// </summary>
public class IpSessionGuardAttribute : AuthGuard
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
        return !string.IsNullOrWhiteSpace(sessionData.SessionId);
    }
}