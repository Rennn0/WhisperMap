using Microsoft.AspNetCore.Mvc;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Shared.Utils;
using ApplicationException = Realtime.Exceptions.ApplicationException;

namespace Realtime.Api.Payment;

public static partial class Api
{
    private static void MapPaymentProvidersApi(this RouteGroupBuilder route)
    {
        route.MapGet("/payments",
                async ([FromServices] IPaymentProviderRepo paymentProviderRepo) =>
                await paymentProviderRepo.GetAsync(new PaymentProvider(), sbyte.MaxValue))
            .RequireAuthorization()
            .WithTags("payment")
            .WithSummary("all available payment providers");

        route.MapGet("/payments/{id}",
                async ([FromRoute] string id, [FromServices] IPaymentProviderRepo paymentProviderRepo) =>
                await paymentProviderRepo.GetAsync(new PaymentProvider { ObjId = id }, sbyte.MinValue))
            .RequireAuthorization()
            .WithTags("payment")
            .WithSummary("payment provider by id");

        route.MapGet("/payments/token",
                async (HttpContext context, [FromServices] TokenProvider tokenProvider,
                    [FromServices] IAuthorizationRepo authRepo, IWebHostEnvironment hostEnvironment) =>
                {
                    if (hostEnvironment.IsDevelopment())
                        return tokenProvider.Create("b7dc9fb35e998892e77fdccf", "dev", "dev@xati.org",
                            permissions: [Permissions.PaymentCreate]);

                    string? sessionCookie = context.Request.Cookies["__xc_se"];
                    string? userIdCookie = context.Request.Cookies["__xc_uid"];

                    Console.WriteLine($"{sessionCookie} {userIdCookie}");

                    if (string.IsNullOrEmpty(sessionCookie) || string.IsNullOrEmpty(userIdCookie))
                        throw new ApplicationException(StatusCodes.Status400BadRequest);

                    AuthorizationInfo? authInfo = await authRepo.SelectAsync(userIdCookie, CancellationToken.None);
                    if (authInfo is not { AccountEnabled: true, VerifiedEmail: true, Email.Length: > 0 })
                        throw new ApplicationException(StatusCodes.Status204NoContent);
                        
                    return tokenProvider.Create(userIdCookie, authInfo.Username, authInfo.Email,
                        permissions: [Permissions.PaymentCreate]);
                })
            .WithTags("payment");
    }
}