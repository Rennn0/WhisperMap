using XatiCraft.ApiContracts;
using XatiCraft.Handlers.Api;
using XatiCraft.Handlers.Upload;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Postgres.XatiCraft;

namespace XatiCraft.Handlers.Impl;

/// <inheritdoc />
internal class UploadProductFileHandler : IUploadProductFileHandler
{
    // private const long MaxFileSize = 5 * 1024 * 1024; //#NOTE ~ 5mb
    private readonly IProductMetadaRepo _productMetadaRepo;
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
        _productMetadaRepo = productMetadaRepos.First(pm => pm is ProductMetadataRepo);
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
        await _productMetadaRepo.InsertAsync(
            new ProductMetadata(uploadResult.OriginalFileName, uploadResult.Key, uploadResult.Location,
                context.Product, context.Order), cancellationToken);
        return new UploadProductFileContract(context);
    }

    /// <inheritdoc />
    public async ValueTask<ApiContract> HandleAsync(GetSignedUrlContext context, CancellationToken cancellationToken)
    {
        if (!await _productRepos.ExistsAsync(context.Product, cancellationToken))
            return new Error(ErrorCode.ArgumentMissmatchInDatabase, Hint: nameof(context.Product));
        SignedUrlUploadResult uploadResult = await _uploader.GetSignedUrlAsync(context.FileName, cancellationToken);
        await _productMetadaRepo.InsertAsync(
            new ProductMetadata(uploadResult.OriginalFileName, uploadResult.Key, uploadResult.Location,
                context.Product, context.Order), cancellationToken);
        return new GetSignedUrlContract(uploadResult.SignedUrl, context);
    }
}