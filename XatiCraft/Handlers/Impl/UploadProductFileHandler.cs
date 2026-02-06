using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Data.Repos;
using XatiCraft.Data.Repos.EfCoreImpl;
using XatiCraft.Handlers.Api;
using XatiCraft.Handlers.Upload;

namespace XatiCraft.Handlers.Impl;

/// <inheritdoc />
internal class UploadProductFileHandler : IUploadProductFileHandler
{
    // private const long MaxFileSize = 5 * 1024 * 1024; //#NOTE ~ 5mb
    private readonly IProductMetadaRepo _productMetadaRepos;
    private readonly IProductRepo _productRepos;
    private readonly IUploader _uploader;


    /// <summary>
    /// </summary>
    /// <param name="uploader"></param>
    /// <param name="productMetadaRepos"></param>
    /// <param name="productRepos"></param>
    public UploadProductFileHandler(IUploader uploader, IEnumerable<IProductMetadaRepo> productMetadaRepos,
        IEnumerable<IProductRepo> productRepos)
    {
        _uploader = uploader;
        _productMetadaRepos = productMetadaRepos.First(pm => pm is ProductMetadataRepo);
        _productRepos = productRepos.First(p => p is ProductRepo);
    }


    /// <inheritdoc />
    public async ValueTask<ApiContract> HandleAsync(UploadProductFileContext context,
        CancellationToken cancellationToken)
    {
        // if (context.Stream.Length > MaxFileSize) return new Error(ErrorCode.FileTooLarge);
        if (!await _productRepos.ExistsAsync(context.Product, cancellationToken))
            return new Error(ErrorCode.ArgumentMissmatchInDatabase, Hint: nameof(context.Product));
        UploadResult uploadResult =
            await _uploader.UploadFileAsync(context.Stream, context.FileName, cancellationToken);
        await _productMetadaRepos.InsertAsync(
            new ProductMetadata(uploadResult.OriginalFileName, uploadResult.Key, uploadResult.Location,
                context.Product), cancellationToken);
        return new UploadProductFileContract(context);
    }

    /// <inheritdoc />
    public async ValueTask<ApiContract> HandleAsync(GetSignedUrlContext context, CancellationToken cancellationToken)
    {
        if (!await _productRepos.ExistsAsync(context.Product, cancellationToken))
            return new Error(ErrorCode.ArgumentMissmatchInDatabase, Hint: nameof(context.Product));
        SignedUrlUploadResult uploadResult = await _uploader.GetSignedUrlAsync(context.FileName, cancellationToken);
        await _productMetadaRepos.InsertAsync(
            new ProductMetadata(uploadResult.OriginalFileName, uploadResult.Key, uploadResult.Location,
                context.Product), cancellationToken);
        return new GetSignedUrlContract(uploadResult.SignedUrl, context);
    }
}