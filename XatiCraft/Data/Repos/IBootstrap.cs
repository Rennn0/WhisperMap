namespace XatiCraft.Data.Repos;

/// <summary>
/// </summary>
public interface IBootstrap
{
    /// <summary>
    /// </summary>
    /// <returns></returns>
    Task RunAsync();

    /// <summary>
    /// </summary>
    void Run();
}