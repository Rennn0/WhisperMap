namespace XatiCraft.Data.Repos;

/// <summary>
/// </summary>
public interface IBootstrap
{
    /// <summary>
    /// </summary>
    /// <returns></returns>
    ValueTask RunAsync();

    /// <summary>
    /// </summary>
    void Run();
}