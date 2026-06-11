using System.ComponentModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Shared.Payment;
using XcLib.Shared.Payment.FlittImpl;
using XcLib.Shared.Payment.FlittImpl.Docs;
using XcLib.Shared.Payment.Interfaces;
using ApplicationException = Realtime.Exceptions.ApplicationException;

namespace Realtime.Api.Payment;

public static partial class Api
{
    private static void MapOrdersApi(this RouteGroupBuilder route)
    {
        route.MapGet("/payments/order", async (
                [FromQuery(Name = "pId")] [Description("selected product id")]
                long productId,
                [FromQuery(Name = "p")] [Description("provider selector")]
                    sbyte provider,
                [FromQuery(Name = "a")] [Description("amount in cents, without comma")]
                    int amount,
                [FromQuery(Name = "d")] [Description("order description")]
                    string description,
                [FromServices] IPaymentProvider paymentProvider,
                [FromServices] IProductOrderRepo orderRepo,
                [FromServices] IProductCartRepo cartRepo,
                ClaimsPrincipal principal) =>
            {
                string? userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) throw new ApplicationException(StatusCodes.Status401Unauthorized);

                ProductCart? cart = await cartRepo.SelectAsync(userId, CancellationToken.None);
                if (cart is not { ProductIds.Count: > 0 } || !cart.ProductIds.Contains(productId.ToString()))
                    return null;

                CreatedOrder order = provider switch
                {
                    1 or 2 or 3 => await paymentProvider.CreateOrderAsync(new CreateRedirectOrderArgs(amount,
                        Currency.Gel,
                        description)),
                    _ => throw new ApplicationException(StatusCodes.Status400BadRequest)
                };

                if (order is not CreatedRedirectOrder redirectedOrder) return order;

                ProductOrder addedOrder = await orderRepo.AddAsync(
                    new ProductOrder(userId, productId, provider, amount)
                    {
                        CheckoutUrl = redirectedOrder.CheckoutUrl,
                        InternalOrderId = redirectedOrder.InternalOrderId
                    });
                await cartRepo.UpsertAsync(new ProductCart(userId)
                    {
                        ProductOrderIds =
                        [
                            addedOrder.ObjId ?? ""
                        ]
                    },
                    CancellationToken.None);

                return order;
            })
            .RequireAuthorization(policy => policy.RequireClaim(Permissions.ClaimPermission, Permissions.PaymentCreate))
            .WithTags("payment", "order")
            .WithSummary("create order and get payment url");

        route.MapGet("/payments/{orderId}/status",
            async (
                [FromRoute] [Description("order id returned from create method")]
                string orderId,
                [FromQuery(Name = "p")] [Description("provider selector")]
                sbyte provider,
                [FromServices] IPaymentProvider paymentProvider) => provider switch
            {
                1 or 2 or 3 => await paymentProvider.GetOrderStatusAsync(new GetRedirectOrderStatusArgs(orderId)),
                _ => throw new ApplicationException(StatusCodes.Status400BadRequest)
            }
            )
            .RequireAuthorization()
            .WithTags("payment", "order")
            .WithSummary("created order's status");
    }
}