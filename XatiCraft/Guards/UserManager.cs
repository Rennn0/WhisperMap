namespace XatiCraft.Guards;

/// <summary>
/// </summary>
public class UserManager : AuthGuard
{
    /// <summary>
    /// </summary>
    /// <param name="sessionData"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="fromHeader"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    protected override bool Validate(SessionData sessionData, IServiceProvider serviceProvider,
        Func<string, string?> fromHeader)
    {
        throw new NotImplementedException();
    }
}