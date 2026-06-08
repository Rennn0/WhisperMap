using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
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
                    [FromQuery(Name = "p")] [Description("provider selector")]
                    sbyte provider,
                    [FromQuery(Name = "a")] [Description("amount in cents, without comma")]
                    int amount,
                    [FromQuery(Name = "d")] [Description("order description")]
                    string description,
                [FromServices] IPaymentProvider paymentProvider) =>
            provider switch
            {
                1 or 2 or 3 => await paymentProvider.CreateOrderAsync(new CreateRedirectOrderArgs(amount, Currency.Gel,
                    description)),
                _ => throw new ApplicationException(StatusCodes.Status400BadRequest)
            })
            .RequireAuthorization()
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