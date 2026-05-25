using Microsoft.AspNetCore.Mvc;
using Webhook.Objects;
using XcLib.Shared.Reactive.Interfaces;

namespace Webhook;

public static partial class Program
{
    private static void ApiPostWebhook(this RouteGroupBuilder builder)
    {
        builder.MapPost("{idx:int}",
            ([FromRoute] int idx, [FromBody] DockerWebhookRequest request,
                [FromServices] IReactiveBus<DockerWebhookRequest> bus) =>
            {
                if (request is not { Push.Tag: "latest" }) return Results.BadRequest();
                request.Index = (sbyte)idx;
                bus.Publish(request);
                
                return Results.Ok(new
                {
                    success = true,
                    idx,
                    started = true
                });
            });
    }
}