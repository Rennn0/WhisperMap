using XatiCraft.ApiContracts;
using XatiCraft.Handlers.Api;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Mongo.XatiCraft;
using ProductRepo = XcLib.Data.Postgres.XatiCraft.ProductRepo;

namespace XatiCraft.Handlers.Impl;

internal class GetProductsCartHandler : IGetProductsHandler
{
    private readonly IProductOrderRepo _productOrderRepo;
    private readonly IProductCartRepo _cartMongo;
    private readonly IProductRepo _productRepo;

    public GetProductsCartHandler(
        IEnumerable<IProductCartRepo> cartRepos,
        IEnumerable<IProductRepo> productRepos,
        IProductOrderRepo productOrderRepo
    )
    {
        _productOrderRepo = productOrderRepo;
        _cartMongo = cartRepos.First(c => c is ProductCartRepoAdapter);
        _productRepo = productRepos.First(p => p is ProductRepo);
    }

    public async ValueTask<ApiContract> HandleAsync(
        GetProductsContext context,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrEmpty(context.UserId))
            return new ApiContract(context);

        ProductCart? cart = await _cartMongo.SelectAsync(context.UserId, cancellationToken);

        if (cart is not { ProductIds.Count: > 0 })
            return new ApiContract(context);

        Dictionary<string, string> productOrder = cart.ProductOrderIds?
            .Select(po => po.Split('_'))
            .ToDictionary(x => x[0], x => x[1])
            .Where(kvp => cart.ProductIds.Contains(kvp.Key))
            .ToDictionary() ?? [];

        List<ProductOrder> orders = (await Task.WhenAll(productOrder.Select(po =>
                _productOrderRepo.GetAsync(new ProductOrder { ObjId = po.Value }, 0, cancellationToken))))
            .SelectMany(x => x).DistinctBy(x => x.ObjId).ToList();
        
        List<Product> products = await _productRepo.GetAsync(
            cart.ProductIds.Select(long.Parse),
            cursor: new GetProductsGeneralHandler.Cursor
            {
                BatchSize = (uint)cart.ProductIds.Count
            },
            cancellationToken: cancellationToken
        );

        GetProductsContract contract = new GetProductsContract(
            products.Select(p => new Product(p.Title, p.Description, p.Price)
            {
                Id = p.Id,
                PreviewImg = p
                    .ProductMetadata?.Where(pm => !string.IsNullOrEmpty(pm.Location))
                    .MinBy(pm => pm.Id)
                    ?.Location,
                Orders = orders.Where(o => o.ProductId == p.Id).Select(x => new ProductOrder
                {
                    Amount = x.Amount,
                    OrderStatus = x.OrderStatus,
                    CheckoutUrl = x.CheckoutUrl,
                    Paid = x.OrderStatus == "approved",
                    Expired = x.OrderStatus == "expired"
                }).ToList()
            }),
            context
        );
        
        return contract;
    }
}
