namespace XatiCraft.Handlers.Upload;

public interface IUploader
{
    Task<UploadResult> UploadFileAsync(Stream fileStream, string fileName, CancellationToken cancellation);
}