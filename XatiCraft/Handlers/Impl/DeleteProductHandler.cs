using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Data.Repos;
using XatiCraft.Data.Repos.EfCoreImpl;
using XatiCraft.Handlers.Api;
using XatiCraft.Handlers.Upload;

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
        Product product = await _productRepo.SelectProductAsync(context.Product, cancellationToken) ??
                          throw new Exception();

        List<Task> tasks = product.ProductMetadata
            ?.Select(pm => _uploader.DeleteObjectAsync(pm.FileKey, cancellationToken)).ToList() ?? [];
        tasks.Add(_ = _productRepo.DeleteAsync(context.Product, cancellationToken));
        await Task.WhenAll(tasks);
        return new ApiContract(context);
    }
}