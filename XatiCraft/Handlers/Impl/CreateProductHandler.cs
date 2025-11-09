using XatiCraft.ApiContracts;
using XatiCraft.Data.Repos;
using XatiCraft.Handlers.Api;
using XatiCraft.Objects;

namespace XatiCraft.Handlers.Impl;

/// <summary>
/// </summary>
public class CreateProductHandler : ICreateProductHandler
{
    private readonly IProductRepo _productRepo;

    /// <summary>
    /// </summary>
    /// <param name="productRepo"></param>
    public CreateProductHandler(IProductRepo productRepo)
    {
        _productRepo = productRepo;
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<ApiContract> HandleAsync(CreateProductContext context,
        CancellationToken cancellationToken)
    {
        Product product = await _productRepo.InsertAsync(new Product(context.Title, context.Description, context.Price),
            cancellationToken);

        ArgumentNullException.ThrowIfNull(product.Id);
        return new CreateProductContract((long)product.Id) { RequestId = context.RequestId };
    }
}