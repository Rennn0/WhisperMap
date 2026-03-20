using XatiCraft.ApiContracts;
using XatiCraft.Data.Objects;
using XatiCraft.Data.Repos;
using XatiCraft.Data.Repos.MongoImpl;
using XatiCraft.Handlers.Api;
using ProductRepo = XatiCraft.Data.Repos.EfCoreImpl.ProductRepo;

namespace XatiCraft.Handlers.Impl;

internal class GetProductsCartHandler : IGetProductsHandler
{
    private readonly IProductCartRepo _cartMongo;
    private readonly IProductRepo _productRepo;
    private readonly HttpContext _httpContext;

    public GetProductsCartHandler(IEnumerable<IProductCartRepo> cartRepos, IEnumerable<IProductRepo> productRepos,
        IHttpContextAccessor httpContextAccessor)
    {
        _cartMongo = cartRepos.First(c => c is ProductCartRepo);
        _productRepo = productRepos.First(p => p is ProductRepo);
        _httpContext = httpContextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async ValueTask<ApiContract> HandleAsync(GetProductsContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(context.UserId))
            return new ApiContract(context);

        ProductCart? cart = await _cartMongo.SelectAsync(context.UserId, cancellationToken);

        if (cart is null)
            return new ApiContract(context);
        
        List<Product> products =
            await _productRepo.SelectAsync(cart.ProductIds.Select(long.Parse), cancellationToken);
        GetProductsContract contract = new GetProductsContract(products.Select(p =>
            new Product(p.Title, p.Description, p.Price)
            {
                Id = p.Id,
                PreviewImg = p.ProductMetadata?.Where(pm => !string.IsNullOrEmpty(pm.Location)).MinBy(pm => pm.Id)
                    ?.Location //#NOTE maybe add order priority?
            }), context);
        return contract;
    }
}