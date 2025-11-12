namespace XatiCraft.Handlers.Upload;

/// <summary>
/// </summary>
/// <param name="OriginalFileName"></param>
/// <param name="Key"></param>
/// <param name="Folder"></param>
/// <param name="Location"></param>
public record UploadResult(string OriginalFileName, string Key, string Folder, string Location);

/// <inheritdoc />
public record SignedUrlUploadResult(
    string SignedUrl,
    string OriginalFileName,
    string Key,
    string Folder,
    string Location) : UploadResult(OriginalFileName, Key, Folder, Location);