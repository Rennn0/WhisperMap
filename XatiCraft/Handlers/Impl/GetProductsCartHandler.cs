using System.Collections.Concurrent;
using XatiCraft.ApiContracts;
using XatiCraft.Handlers.Api;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Data.Mongo.XatiCraft;
using XcLib.Shared.Payment;
using XcLib.Shared.Payment.FlittImpl;
using XcLib.Shared.Payment.Interfaces;
using ProductRepo = XcLib.Data.Postgres.XatiCraft.ProductRepo;

namespace XatiCraft.Handlers.Impl;

internal class GetProductsCartHandler : IGetProductsHandler
{
    private readonly IProductOrderRepo _productOrderRepo;
    private readonly IPaymentProvider _paymentProvider;
    private readonly IProductCartRepo _cartMongo;
    private readonly IProductRepo _productRepo;

    public GetProductsCartHandler(
        IEnumerable<IProductCartRepo> cartRepos,
        IEnumerable<IProductRepo> productRepos,
        IProductOrderRepo productOrderRepo,
        IPaymentProvider paymentProvider
    )
    {
        _productOrderRepo = productOrderRepo;
        _paymentProvider = paymentProvider;
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

        IEnumerable<IGrouping<string, string[]>> productOrderIds = cart.ProductOrderIds?
            .Select(po => po.Split('_'))
            .GroupBy(x => x[0])
            .Where(kvp => cart.ProductIds.Contains(kvp.Key)) ?? [];

        ConcurrentDictionary<string, List<ProductOrder>> productOrderes =
            new ConcurrentDictionary<string, List<ProductOrder>>();
        await Parallel.ForEachAsync(productOrderIds, cancellationToken, async (group, token) =>
        {
            foreach (string[] strings in group)
            {
                ProductOrder? order =
                    (await _productOrderRepo.GetAsync(new ProductOrder { ObjId = strings[1] }, 0, token))
                    .FirstOrDefault();
                if (order is { InternalOrderId: null }) continue;
                OrderStatus orderStatus =
                    await _paymentProvider.GetOrderStatusAsync(new GetRedirectOrderStatusArgs(order!.InternalOrderId),
                        token);
                if (orderStatus is not RedirectedOrderStatus ros) continue;
                order.OrderStatus ??= _paymentProvider.MapStatus(ros.Status).ToString();
                if (productOrderes.TryGetValue(strings[0], out List<ProductOrder>? list))
                    list.Add(order);
                else
                    productOrderes[strings[0]] = [order];
            }
        });
        
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
                Orders = productOrderes.TryGetValue(p.Id.ToString() ?? "", out List<ProductOrder>? o)
                    ? o.Select(x => new ProductOrder
                {
                    Amount = x.Amount,
                    OrderStatus = x.OrderStatus,
                    CheckoutUrl = x.CheckoutUrl,
                }).ToList()
                    : []
            }),
            context
        );
        
        return contract;
    }
}
