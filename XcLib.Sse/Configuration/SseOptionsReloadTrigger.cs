namespace XcLib.Sse.Configuration;

public sealed class SseOptionsReloadTrigger : IConfigurationTrigger
{
    private event Action? ReloadRequested;

    public void Register(Action action) => ReloadRequested = action;

    public void Invoke() => ReloadRequested?.Invoke();
}