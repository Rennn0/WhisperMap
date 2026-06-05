namespace Realtime.Api.Payment;

public static partial class Api
{
    public static RouteGroupBuilder MapPaymentApi(this RouteGroupBuilder route)
    {
        route.MapPaymentProvidersApi();
        route.MapOrdersApi();
        return route;
    }
}