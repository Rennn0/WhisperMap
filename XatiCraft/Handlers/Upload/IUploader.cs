namespace XatiCraft.Handlers.Upload;

/// <summary>
/// </summary>
public interface IUploader
{
    /// <summary>
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="fileName"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<UploadResult> UploadFileAsync(Stream fileStream, string fileName, CancellationToken cancellation);
}