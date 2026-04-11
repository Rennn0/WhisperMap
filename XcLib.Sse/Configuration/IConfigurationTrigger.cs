namespace XcLib.Sse.Configuration;

public interface IConfigurationTrigger
{
    void Register(Action action);
    void Invoke();
}