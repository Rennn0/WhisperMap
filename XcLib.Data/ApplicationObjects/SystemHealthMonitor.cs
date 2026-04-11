namespace XcLib.Data.ApplicationObjects;

/// <summary>
/// </summary>
public class SystemHealthMonitor
{
    /// <summary>
    /// </summary>
    public DateTimeOffset Start { get; } = DateTimeOffset.Now;

    /// <summary>
    /// </summary>
    public TimeSpan Uptime => DateTimeOffset.Now - Start;
}