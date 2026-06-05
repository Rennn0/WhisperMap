using Microsoft.AspNetCore.Mvc;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;

namespace Realtime;

public static partial class Program
{
    public static void ApiGetAvailableCheckouts(this RouteGroupBuilder route)
    {
        route.MapGet("/checkouts",
            async ([FromServices] IPaymentProviderRepo paymentProviderRepo) =>
            await paymentProviderRepo.GetAsync(new PaymentProvider(), sbyte.MaxValue));

        route.MapGet("/checkouts/{id}",
            async ([FromRoute] string id, [FromServices] IPaymentProviderRepo paymentProviderRepo) =>
            await paymentProviderRepo.GetAsync(new PaymentProvider { ObjId = id }, sbyte.MinValue));
    }
}