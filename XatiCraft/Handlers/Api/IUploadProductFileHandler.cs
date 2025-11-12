using XatiCraft.ApiContracts;

namespace XatiCraft.Handlers.Api;

/// <summary>
/// </summary>
public interface IUploadProductFileHandler
{
    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<ApiContract> HandleAsync(UploadProductFileContext context, CancellationToken cancellationToken);

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<ApiContract> HandleAsync(GetSignedUrlContext context, CancellationToken cancellationToken);
}