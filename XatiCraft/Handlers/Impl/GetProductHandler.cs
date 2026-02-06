using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Data.Repos;
using XatiCraft.Data.Repos.EfCoreImpl;
using XatiCraft.Handlers.Api;

namespace XatiCraft.Handlers.Impl;

/// <summary>
/// </summary>
internal class GetProductHandler : IGetProductHandler
{
    private readonly IProductCartHandler _productCartHandler;
    private readonly IProductRepo _productRepos;

    /// <summary>
    /// </summary>
    /// <param name="productRepos"></param>
    /// <param name="productCartHandler"></param>
    public GetProductHandler(IEnumerable<IProductRepo> productRepos, IProductCartHandler productCartHandler)
    {
        _productRepos = productRepos.First(p => p is ProductRepo);
        _productCartHandler = productCartHandler;
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<ApiContract> HandleAsync(GetProductContext context, CancellationToken cancellationToken)
    {
        Product? product = await _productRepos.SelectProductAsync(context.ProductId, cancellationToken);
        if (product is null) return new Error(ErrorCode.ArgumentMissmatchInDatabase);

        bool inCart = ((ProductCartHandler)_productCartHandler).ExistsInCart(product);

        return new GetProductContract(context.ProductId, product.Title, product.Description,
            product.Price, inCart, product.ProductMetadata?.Select(pmd => pmd.Location), context);
    }
}