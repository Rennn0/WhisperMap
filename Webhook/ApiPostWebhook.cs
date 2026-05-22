using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Webhook.Meshes;
using Webhook.Objects;

namespace Webhook;

public static partial class Program
{
    private static void ApiPostWebhook(this RouteGroupBuilder builder)
    {
        builder.MapPost("{idx:int}",
            async ([FromRoute] int idx, [FromBody] DockerWebhookRequest request, [FromServices] WebhookMesh mesh ) =>
            {
                if (request is not { Push.Tag: "latest" }) return Results.BadRequest();
                request.Index = (sbyte)idx;
                await mesh.PublishAsync(request,CancellationToken.None);
                
                return Results.Ok(new
                {
                    success = true,
                    idx,
                    started = true
                });
            });
    }
}