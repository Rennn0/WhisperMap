using XatiCraft.ApiContracts;
using XatiCraft.Data.Repos;
using XatiCraft.Handlers.Api;
using XatiCraft.Handlers.Upload;
using XatiCraft.Objects;

namespace XatiCraft.Handlers.Impl;

/// <summary>
/// </summary>
public class UploadProductFileHandler : IUploadProductFileHandler
{
    // private const long MaxFileSize = 5 * 1024 * 1024; //#NOTE ~ 5mb
    private readonly IProductMetadaRepo _productMetadaRepo;
    private readonly IProductRepo _productRepo;
    private readonly IUploader _uploader;

    /// <summary>
    /// </summary>
    /// <param name="uploader"></param>
    /// <param name="productMetadaRepo"></param>
    /// <param name="productRepo"></param>
    public UploadProductFileHandler(IUploader uploader, IProductMetadaRepo productMetadaRepo, IProductRepo productRepo)
    {
        _uploader = uploader;
        _productMetadaRepo = productMetadaRepo;
        _productRepo = productRepo;
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<ApiContract> HandleAsync(UploadProductFileContext context,
        CancellationToken cancellationToken)
    {
        // if (context.Stream.Length > MaxFileSize) return new Error(ErrorCode.FileTooLarge);
        if (!await _productRepo.ExistsAsync(context.Product, cancellationToken))
            return new Error(ErrorCode.ArgumentMissmatchInDatabase, Hint: nameof(context.Product));

        UploadResult uploadResult =
            await _uploader.UploadFileAsync(context.Stream, context.FileName, cancellationToken);
        await _productMetadaRepo.InsertAsync(
            new ProductMetadata(uploadResult.OriginalFileName, uploadResult.Key, uploadResult.Location,
                context.Product), cancellationToken);
        return new UploadProductFileContract { RequestId = context.RequestId };
    }
}