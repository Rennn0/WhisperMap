using Microsoft.AspNetCore.Mvc;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Shared.Utils;

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
                    [FromServices] IAuthorizationRepo authRepo, IWebHostEnvironment hostEnvironment,
                    ILoggerFactory loggerFactory) =>
                {
                    if (hostEnvironment.IsDevelopment())
                        return Results.Text(tokenProvider.Create("b7dc9fb35e998892e77fdccf", "dev", "dev@xati.org",
                            permissions: [Permissions.PaymentCreate]));

                    ILogger logger = loggerFactory.CreateLogger("payments/token");
                    string? sessionCookie = GetLatestVersionCookie("__xc_se");
                    string? userIdCookie = GetLatestVersionCookie("__xc_uid");

                    logger.LogInformation("{SessionCookie} {UserIdCookie}", sessionCookie, userIdCookie);

                    if (string.IsNullOrEmpty(sessionCookie) || string.IsNullOrEmpty(userIdCookie))
                        return Results.StatusCode(StatusCodes.Status406NotAcceptable);

                    AuthorizationInfo? authInfo = await authRepo.SelectAsync(userIdCookie, CancellationToken.None);
                    if (authInfo is not { AccountEnabled: true, VerifiedEmail: true, Email.Length: > 0 })
                        return Results.StatusCode(StatusCodes.Status418ImATeapot);

                    return Results.Text(tokenProvider.Create(userIdCookie, authInfo.Username, authInfo.Email,
                        permissions: [Permissions.PaymentCreate]));

                    string? GetLatestVersionCookie(string key) => context.Request.Cookies
                        .Where(c => c.Key.StartsWith(key))
                        .Select(c =>
                        {
                            string version = c.Key.Split('_')[^1];
                            return (version, c.Value);
                        }).OrderByDescending(c => c.version)
                        .Select(c => c.Value)
                        .FirstOrDefault();
                })
            .WithTags("payment");
    }
}