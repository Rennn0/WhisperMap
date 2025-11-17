using XatiCraft.ApiContracts;
using XatiCraft.Data.Repos;
using XatiCraft.Handlers.Api;
using XatiCraft.Handlers.Upload;
using XatiCraft.Objects;

namespace XatiCraft.Handlers.Impl;

/// <inheritdoc />
public class DeleteProductHandler : IDeleteProductHandler
{
    private readonly IProductRepo _productRepo;
    private readonly IUploader _uploader;

    /// <summary>
    /// </summary>
    /// <param name="productRepo"></param>
    /// <param name="uploader"></param>
    public DeleteProductHandler(IProductRepo productRepo, IUploader uploader)
    {
        _productRepo = productRepo;
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
        return new ApiContract { RequestId = context.RequestId };
    }
}