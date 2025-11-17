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

    /// <summary>
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<SignedUrlUploadResult> GetSignedUrlAsync(string fileName, CancellationToken cancellation);

    /// <summary>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task DeleteObjectAsync(string key, CancellationToken cancellation);
}