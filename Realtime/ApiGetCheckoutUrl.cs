using Microsoft.AspNetCore.Mvc;
using XcLib.Shared.Payment.FlittImpl;
using XcLib.Shared.Payment.FlittImpl.Docs;
using XcLib.Shared.Payment.Interfaces;

namespace Realtime;

public static partial class Program
{
    private static void ApiGetCheckoutUrl(this RouteGroupBuilder route) =>
        route.MapGet("/checkout", async (
                [FromQuery(Name = "p")] sbyte provider,
                [FromQuery(Name = "a")] int amount,
                [FromQuery(Name = "d")] string description,
                [FromServices] IPaymentProvider paymentProvider) =>
            provider switch
            {
                1 => await paymentProvider.CreateOrderAsync(new CreateRedirectOrderArgs(amount, Currency.Gel,
                    description)),
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
            });
}