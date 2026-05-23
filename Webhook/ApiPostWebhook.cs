using Microsoft.AspNetCore.Mvc;
using Webhook.Meshes.Webhook;
using Webhook.Objects;

namespace Webhook;

public static partial class Program
{
    private static void ApiPostWebhook(this RouteGroupBuilder builder)
    {
        builder.MapPost("{idx:int}",
            ([FromRoute] int idx, [FromBody] DockerWebhookRequest request, [FromServices] WebhookMesh webhookMesh) =>
            {
                if (request is not { Push.Tag: "latest" }) return Results.BadRequest();
                request.Index = (sbyte)idx;
                webhookMesh.Publish(request);
                
                return Results.Ok(new
                {
                    success = true,
                    idx,
                    started = true
                });
            });
    }
}