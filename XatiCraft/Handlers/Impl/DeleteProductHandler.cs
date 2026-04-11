using XatiCraft.ApiContracts;
using XatiCraft.Handlers.Api;
using XatiCraft.Handlers.Upload;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Postgres.XatiCraft;

namespace XatiCraft.Handlers.Impl;

/// <inheritdoc />
internal class DeleteProductHandler : IDeleteProductHandler
{
    private readonly IProductRepo _productRepo;
    private readonly IUploader _uploader;

    /// <summary>
    /// </summary>
    /// <param name="productRepos"></param>
    /// <param name="uploader"></param>
    public DeleteProductHandler(IEnumerable<IProductRepo> productRepos, IUploader uploader)
    {
        _productRepo = productRepos.First(p => p is ProductRepo);
        _uploader = uploader;
    }

    /// <inheritdoc />
    public async ValueTask<ApiContract> HandleAsync(DeleteProductContext context, CancellationToken cancellationToken)
    {
        Product product = await _productRepo.SelectAsync(context.ProductId, cancellationToken) ??
                          throw new Exception();

        List<Task> tasks = product.ProductMetadata
            ?.Select(pm => _uploader.DeleteObjectAsync(pm.FileKey, cancellationToken)).ToList() ?? [];
        tasks.Add(_productRepo.DeleteAsync(context.ProductId, cancellationToken));
        await Task.WhenAll(tasks);
        return new ApiContract(context);
    }
}