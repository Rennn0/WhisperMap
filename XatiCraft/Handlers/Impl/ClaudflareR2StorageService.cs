using System.Security.Cryptography;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using XatiCraft.Handlers.Read;
using XatiCraft.Handlers.Upload;
using XatiCraft.Settings;

namespace XatiCraft.Handlers.Impl;

/// <summary>
/// </summary>
public class ClaudflareR2StorageService : IUploader, IReader
{
    private readonly AmazonS3Client _s3Client;
    private readonly ClaudflareR2Settings _settings;

    /// <summary>
    /// </summary>
    /// <param name="settings"></param>
    public ClaudflareR2StorageService(IOptionsSnapshot<ClaudflareR2Settings> settings)
    {
        _settings = settings.Value;
        _s3Client = new AmazonS3Client(_settings.AccessKey, _settings.SecretKey, new AmazonS3Config
        {
            ServiceURL = _settings.ServiceUrl,
            ForcePathStyle = true
        });
    }

    /// <summary>
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task<IEnumerable<string>> ListFilesAsync(string folder, CancellationToken cancellation)
    {
        string prefix = string.IsNullOrEmpty(folder) ? "" : $"{folder}/";
        ListObjectsV2Request request = new ListObjectsV2Request
        {
            BucketName = _settings.Bucket,
            Prefix = prefix
        };

        ListObjectsV2Response? response = await _s3Client.ListObjectsV2Async(request, cancellation);
        return response is { S3Objects.Count: > 0 }
            ? response.S3Objects.Select(o => $"{_settings.PublicUrl}/{o.Key}")
            : [];
    }

    /// <summary>
    /// </summary>
    /// <param name="fileStream"></param>
    /// <param name="fileName"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task<UploadResult> UploadFileAsync(Stream fileStream, string fileName, CancellationToken cancellation)
    {
        if (fileStream.CanSeek) fileStream.Position = 0;

        string folder = Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".mp4" => "video",
            ".mov" => "video",
            ".mkv" => "video",
            ".jpg" => "photo",
            ".jpeg" => "photo",
            ".png" => "photo",
            _ => "misc"
        };

        StringBuilder keyBuilder = new StringBuilder();
        keyBuilder.Append(folder);
        keyBuilder.Append('/');
        keyBuilder.Append(NormalizeFileName(fileName));

        PutObjectRequest request = new PutObjectRequest
        {
            BucketName = _settings.Bucket,
            Key = keyBuilder.ToString(),
            InputStream = fileStream,
            AutoCloseStream = false,
            UseChunkEncoding = false,
            Headers = { ContentLength = fileStream.Length }
        };

        PutObjectResponse response = await _s3Client.PutObjectAsync(request, cancellation);
        string location = $"{_settings.PublicUrl}/{keyBuilder}";
        UploadResult uploadResult = new UploadResult(fileName, keyBuilder.ToString(), folder, location);
        return uploadResult;
    }

    private static string NormalizeFileName(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        string name = Path.GetFileNameWithoutExtension(fileName);

        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(name + DateTime.UtcNow.Ticks));
        string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return $"{hash}{extension}";
    }
}