namespace XatiCraft.Handlers.Read;

public interface IReader
{
    Task<IEnumerable<string>> ListFilesAsync(string folder, CancellationToken cancellation);
}