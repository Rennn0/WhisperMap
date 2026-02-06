namespace XatiCraft.Handlers.Read;

/// <summary>
/// </summary>
public interface IReader
{
    /// <summary>
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<IEnumerable<string>> ListFilesAsync(string folder, CancellationToken cancellation);
}