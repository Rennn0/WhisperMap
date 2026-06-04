using Microsoft.AspNetCore.Mvc;
using XcLib.Shared.Payment.FlittImpl;
using XcLib.Shared.Payment.Interfaces;

namespace Realtime;

public static partial class Program
{
    private static void ApiGetOrderStatus(this RouteGroupBuilder route) => route.MapGet("/status/{orderId}",
        async (
            [FromRoute] string orderId,
            [FromQuery(Name = "p")] sbyte provider,
            [FromServices] IPaymentProvider paymentProvider) => provider switch
        {
            1 => await paymentProvider.GetOrderStatusAsync(new GetRedirectOrderStatusArgs(orderId)),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        }
    );
}