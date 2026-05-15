using XatiCraft.ApiContracts;
using XatiCraft.Handlers.Api;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Mongo.XatiCraft;
using ProductRepo = XcLib.Data.Postgres.XatiCraft.ProductRepo;

namespace XatiCraft.Handlers.Impl;

/// <summary>
/// </summary>
internal class GetProductHandler : IGetProductHandler
{
    private readonly IProductCartHandler _productCartHandler;
    private readonly IProductRepo _productRepos;
    private readonly IProductRepo _productObjRepos;
    private readonly IProductCartRepo _productCartMongo;

    /// <summary>
    /// </summary>
    /// <param name="productRepos"></param>
    /// <param name="productCartHandler"></param>
    /// <param name="productCartRepos"></param>
    public GetProductHandler(IEnumerable<IProductRepo> productRepos, IProductCartHandler productCartHandler,
        IEnumerable<IProductCartRepo> productCartRepos)
    {
        IEnumerable<IProductRepo> repos = productRepos.ToList();
        _productRepos = repos.First(p => p is ProductRepo);
        _productObjRepos = repos.First(p => p is XcLib.Data.Mongo.XatiCraft.ProductRepo);
        _productCartHandler = productCartHandler;
        _productCartMongo = productCartRepos.First(x => x is ProductCartRepo);
    }

    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<ApiContract> HandleAsync(GetProductContext context, CancellationToken cancellationToken)
    {
        Product? product;

        if (long.TryParse(context.Id, out long id))
            product = await _productRepos.SelectAsync(id, cancellationToken);
        else
            product = await _productObjRepos.SelectAsync(context.Id,
                cancellationToken); //#NOTE usually this data mustnt be in nosql storage but anyway
        
        if (product is null) return new Error(ErrorCode.ArgumentMissmatchInDatabase);

        bool inCart;
        if (string.IsNullOrEmpty(context.UserId))
            inCart = ((GetProductCartCookieHandler)_productCartHandler).ExistsInCart(product);
        else
            inCart =
                (await _productCartMongo.SelectAsync(context.UserId, cancellationToken))?.ProductIds.Contains(
                    context.Id) ?? false;

        return new GetProductContract(context.Id, product.Title, product.Description,
            product.Price, inCart, product.ProductMetadata?.OrderBy(x => x.Order).Select(pmd => pmd.Location), context);
    }
}