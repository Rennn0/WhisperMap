using System.Collections.Concurrent;
using System.Collections.Immutable;
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
        _cartMongo = cartRepos.First(c => c is ProductCartRepo);
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

        IEnumerable<IGrouping<string, string[]>> productOrderIds = cart.ProductIdOrderId?
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
                switch (order)
                {
                    case { InternalOrderId: null } or null:
                        continue;
                    case { OrderStatus: null }:
                    {
                        OrderStatus orderStatus =
                            await _paymentProvider.GetOrderStatusAsync(
                                new GetRedirectOrderStatusArgs(order!.InternalOrderId),
                                token);
                        if (orderStatus is not RedirectedOrderStatus ros) continue;
                        order.OrderStatus = _paymentProvider.MapStatus(ros.Status).ToString();
                        break;
                    }
                }

                order.UseLink = _paymentProvider.MapStatus(order.OrderStatus) == AppOrderStatus.None;
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
            products.Select(p =>
            {
                productOrderes.TryGetValue(p.Id.ToString() ?? "", out List<ProductOrder>? o);
                o ??= [];
                return new Product(p.Title, p.Description, p.Price)
                {
                    Id = p.Id,
                    IsPaid = o.Any(v => _paymentProvider.MapStatus(v.OrderStatus) == AppOrderStatus.Paid),
                    PreviewImg = p
                    .ProductMetadata?.Where(pm => !string.IsNullOrEmpty(pm.Location))
                    .MinBy(pm => pm.Id)
                    ?.Location,
                    Orders = o.Select(x => new ProductOrder
                    {
                        OrderStatus = x.OrderStatus,
                        CheckoutUrl = x.CheckoutUrl,
                        UseLink = x.UseLink
                    }).ToList()
                };
            }),
            context
        );
        
        return contract;
    }
}
