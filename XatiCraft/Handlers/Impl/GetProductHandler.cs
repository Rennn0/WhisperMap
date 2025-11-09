using XatiCraft.ApiContracts;
using XatiCraft.Data.Repos;
using XatiCraft.Handlers.Api;
using XatiCraft.Objects;

namespace XatiCraft.Handlers.Impl;

/// <summary>
/// </summary>
public class GetProductHandler : IGetProductHandler
{
    private readonly IProductRepo _productRepo;

    /// <summary>
    /// </summary>
    /// <param name="productRepo"></param>
    public GetProductHandler(IProductRepo productRepo)
    {
        _productRepo = productRepo;
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<ApiContract> HandleAsync(GetProductContext context, CancellationToken cancellationToken)
    {
        Product? product = await _productRepo.SelectProductAsync(context.ProductId, cancellationToken);
        if (product is null) return new Error(ErrorCode.ArgumentMissmatchInDatabase);

        GetProductContract contract = new GetProductContract(context.ProductId, product.Title, product.Description,
            product.Price, product.ProductMetadata?.Select(pmd => pmd.Location));
        return contract;
    }
}