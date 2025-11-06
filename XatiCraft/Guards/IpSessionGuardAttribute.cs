namespace XatiCraft.Guards;

public class IpSessionGuardAttribute : AuthGuard
{
    protected override bool Validate(SessionData sessionData, IServiceProvider serviceProvider,
        Func<string, string?> fromHeader)
    {
        return !string.IsNullOrWhiteSpace(sessionData.SessionId);
    }
}