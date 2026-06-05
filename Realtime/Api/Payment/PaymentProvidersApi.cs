using Microsoft.AspNetCore.Mvc;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;

namespace Realtime.Api.Payment;

public static partial class Api
{
    private static void MapPaymentProvidersApi(this RouteGroupBuilder route)
    {
        route.MapGet("/payments",
                async ([FromServices] IPaymentProviderRepo paymentProviderRepo) =>
                await paymentProviderRepo.GetAsync(new PaymentProvider(), sbyte.MaxValue))
            .WithTags("payment")
            .WithSummary("all available payment providers");

        route.MapGet("/payments/{id}",
                async ([FromRoute] string id, [FromServices] IPaymentProviderRepo paymentProviderRepo) =>
                await paymentProviderRepo.GetAsync(new PaymentProvider { ObjId = id }, sbyte.MinValue))
            .WithTags("payment")
            .WithSummary("payment provider by id");
    }
}